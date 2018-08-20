using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.Service.EthereumCommon.Core.Telemetry;
using Lykke.Service.EthereumWorker.Core.Repositories;
using Lykke.Service.EthereumWorker.Core.Services;
using Microsoft.ApplicationInsights;


namespace Lykke.Service.EthereumWorker.Services
{
    [UsedImplicitly]
    public class BlockchainIndexingService : IBlockchainIndexingService
    {
        private readonly IBalanceObservationTaskRepository _balanceObservationTaskRepository;
        private readonly IBlockchainService _blockchainService;
        private readonly TimeSpan _blockLockDuration;
        private readonly IBlockIndexationLockRepository _blockLockRepository;
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly IBlockchainIndexationStateRepository _stateRepository;
        private readonly TelemetryClient _telemetryClient;
        private readonly ITransactionReceiptRepository _transactionReceiptRepository;

        
        public BlockchainIndexingService(
            IBalanceObservationTaskRepository balanceObservationTaskRepository,
            IBlockchainService blockchainServiceService,
            IBlockIndexationLockRepository blockLockRepository,
            IChaosKitty chaosKitty,
            ILogFactory logFactory,
            Settings settings,
            IBlockchainIndexationStateRepository stateRepository,
            ITransactionReceiptRepository transactionReceiptRepository)
        {
            _balanceObservationTaskRepository = balanceObservationTaskRepository;
            _blockchainService = blockchainServiceService;
            _blockLockDuration = settings.BlockLockDuration;
            _blockLockRepository = blockLockRepository;
            _chaosKitty = chaosKitty;
            _log = logFactory.CreateLog(this);
            _stateRepository = stateRepository;
            _telemetryClient = new TelemetryClient();
            _transactionReceiptRepository = transactionReceiptRepository;
        }


        public async Task<BigInteger[]> GetNonIndexedBlocksAsync(
            int take)
        {
            using (var eventHolder = _telemetryClient.StartEvent("GotNonIndexedBlocks"))
            {
                try
                {
                    var nonIndexedBlocks = new List<BigInteger>(take);
                    var stateLock = await _stateRepository.WaitLockAsync();

                    try
                    {
                        var indexationState = await _stateRepository.GetOrCreateAsync();

                        // Update best block

                        var bestBlockNumber = await _blockchainService.GetBestTrustedBlockNumberAsync();

                        if (indexationState.TryToUpdateBestBlock(bestBlockNumber))
                        {
                            _chaosKitty.Meow("Failed to update indexation state.");

                            await _stateRepository.UpdateAsync(indexationState);

                            _log.Info($"Best block updated to {bestBlockNumber}.");
                        }

                        // Get and clean up block locks

                        var locksExpiredOn = DateTime.UtcNow - _blockLockDuration;

                        var blockLocks = (await _blockLockRepository.GetAsync())
                            .ToList();

                        var expiredBlockLocks = blockLocks
                            .Where(x => x.LockedOn <= locksExpiredOn)
                            .ToList();

                        foreach (var @lock in expiredBlockLocks)
                        {
                            blockLocks.Remove(@lock);

                            _log.Debug($"Releasing expired indexation lock for block {@lock.BlockNumber}...");

                            await ReleaseBlockLockAsync(@lock.BlockNumber);
                        }

                        // Get non-indexed blocks
                        foreach (var blockNumber in indexationState.GetNonIndexedBlockNumbers())
                        {
                            if (blockLocks.All(x => x.BlockNumber != blockNumber))
                            {
                                try
                                {
                                    await _blockLockRepository.InsertOrReplaceAsync(blockNumber);

                                    nonIndexedBlocks.Add(blockNumber);
                                }
                                catch (Exception e)
                                {
                                    _log.Error(e, $"Failed to set indexation lock for block {blockNumber}");
                                }
                            }
                            else
                            {
                                continue;
                            }

                            if (nonIndexedBlocks.Count == take)
                            {
                                break;
                            }
                        }
                    }
                    finally
                    {
                        await stateLock.ReleaseAsync();
                    }

                    _log.Debug
                    (
                        nonIndexedBlocks.Any()
                            ? $"Got non-indexed blocks [{string.Join(", ", nonIndexedBlocks)}]."
                            : "Non-indexed blocks not found."
                    );

                    return nonIndexedBlocks.ToArray();
                }
                catch (Exception e)
                {
                    eventHolder.TrackFailure("FailedToGetNonIndexedBlocks");
                    
                    _log.Error(e, "Failed to get non indexed blocks.");

                    return Array.Empty<BigInteger>();
                }
            }
        }

        public async Task<BigInteger[]> IndexBlocksAsync(
            BigInteger[] blockNumbers)
        {
            using (var eventHolder = _telemetryClient.StartEvent("BlocksIndexed"))
            {
                eventHolder.SetMetric("blocksCount", blockNumbers.Length);
                
                try
                {
                    var indexedBlocks = new ConcurrentBag<BigInteger>();
                    var indexationTasks = blockNumbers.Select
                        (
                            blockNumber => Task.Run(async () =>
                            {
                                if (await IndexBlockAsync(blockNumber))
                                {
                                    indexedBlocks.Add(blockNumber);
                                }
                                else
                                {
                                    await ReleaseBlockLockAsync(blockNumber);
                                }
                            }))
                        .ToList();

                    await Task.WhenAll(indexationTasks);

                    return indexedBlocks.OrderBy(x => x).ToArray();
                }
                catch (Exception e)
                {
                    eventHolder.TrackFailure("FailedToIndexBlocks");
                    
                    _log.Error(e, $"Failed to index blocks [{string.Join(", ", blockNumbers)}].");

                    return Array.Empty<BigInteger>();
                }
            }
        }

        private async Task<bool> IndexBlockAsync(
            BigInteger blockNumber)
        {
            using (var eventHolder = _telemetryClient.StartEvent("BlockIndexed"))
            {
                eventHolder.SetProperty(nameof(blockNumber), blockNumber);
                
                try
                {
                    await _transactionReceiptRepository.ClearBlockAsync(blockNumber);
                
                    var transactionReceipts = (await _blockchainService.GetTransactionReceiptsAsync(blockNumber))
                        .ToList();
            
                    var affectedAddresses = transactionReceipts
                        .Select(x => new[] { x.From, x.To })
                        .SelectMany(x => x)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct();

                    await Task.WhenAll
                    (
                        Task.Run(async () =>
                        {
                            foreach (var receipt in transactionReceipts)
                            {
                                await _transactionReceiptRepository.InsertOrReplaceAsync(receipt);
                            }
                        }),
                        Task.Run(async () =>
                        {
                            foreach (var affectedAddress in affectedAddresses)
                            {
                                await _balanceObservationTaskRepository.EnqueueAsync(new BalanceObservationTask
                                {
                                    Address = affectedAddress
                                });
                            }
                        })
                    );
                
                    _log.Info($"Block [{blockNumber}] has been indexed.");
                
                    return true;
                }
                catch (Exception e)
                {
                    eventHolder.TrackFailure("FailedToIndexBlock");
                    
                    _log.Error(e, $"Failed to index block [{blockNumber}].");
                
                    return false;
                }
            }
        }
        
        public async Task MarkBlocksAsIndexed(
            BigInteger[] blockNumbers)
        {
            using (var eventHolder = _telemetryClient.StartEvent("BlocksMarkedAsIndexed"))
            {
                eventHolder.SetMetric("blocksCount", blockNumbers.Length);
                
                try
                {
                    var stateLock = await _stateRepository.WaitLockAsync();
                
                    try
                    {
                        var indexationState = await _stateRepository.GetOrCreateAsync();
                        var stateUpdateIsNecessary = false;

                        foreach (var blockNumber in blockNumbers)
                        {
                            if (indexationState.TryToMarkBlockAsIndexed(blockNumber))
                            {
                                stateUpdateIsNecessary = true;
                            }
                        }

                        if (stateUpdateIsNecessary)
                        {
                            await _stateRepository.UpdateAsync(indexationState);
                        }
                    }
                    finally
                    {
                        await stateLock.ReleaseAsync();
                    }

                    await Task.WhenAll(blockNumbers.Select(ReleaseBlockLockAsync));

                    if (blockNumbers.Any())
                    {
                        _log.Debug($"Blocks [{string.Join(", ", blockNumbers)}] has been marked as indexed.");
                    }
                }
                catch (Exception e)
                {
                    eventHolder.TrackFailure("FailedToMarkBlocksAsIndexed");
                    
                    _log.Error(e, $"Failed to mark blocks [{string.Join(", ", blockNumbers)}] as indexed.");
                }
            }
        }

        
        private async Task ReleaseBlockLockAsync(
            BigInteger blockNumber)
        {
            using (var eventHolder = _telemetryClient.StartEvent("BlockLockReleased"))
            {
                eventHolder.SetProperty(nameof(blockNumber), blockNumber);
                
                try
                {
                    await _blockLockRepository.DeleteIfExistsAsync(blockNumber);
                
                    _log.Debug($"Block indexation lock for block {blockNumber} has been released.");
                }
                catch (Exception e)
                {
                    eventHolder.TrackFailure("FailedToReleaseBlockLock");
                    
                    _log.Error(e, $"Failed to release indexation lock for block {blockNumber}.");
                }
            }
        }

        public class Settings
        {
            public TimeSpan BlockLockDuration { get; set; }
        }
    }
}

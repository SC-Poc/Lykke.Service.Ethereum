using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.Service.EthereumWorker.Core.Repositories;
using Lykke.Service.EthereumWorker.Core.Services;


namespace Lykke.Service.EthereumWorker.Services
{
    [UsedImplicitly]
    public class BlockchainIndexingService : IBlockchainIndexingService
    {
        private readonly IBalanceObservationTaskRepository _balanceObservationTaskRepository;
        private readonly IBlockchainService _blockchainService;
        private readonly TimeSpan _blockLockDuration;
        private readonly IBlockIndexationLockRepository _blockLockRepository;
        private readonly ILog _log;
        private readonly int _maxDegreeOfParallelism;
        private readonly IBlockchainIndexationStateRepository _stateRepository;
        private readonly ITransactionReceiptRepository _transactionReceiptRepository;

        
        public BlockchainIndexingService(
            IBalanceObservationTaskRepository balanceObservationTaskRepository,
            IBlockchainService blockchainServiceService,
            TimeSpan blockLockDuration,
            IBlockIndexationLockRepository blockLockRepository,
            ILogFactory logFactory,
            int maxDegreeOfParallelism,
            IBlockchainIndexationStateRepository stateRepository,
            ITransactionReceiptRepository transactionReceiptRepository)
        {
            _balanceObservationTaskRepository = balanceObservationTaskRepository;
            _blockchainService = blockchainServiceService;
            _blockLockDuration = blockLockDuration;
            _blockLockRepository = blockLockRepository;
            _log = logFactory.CreateLog(this);
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _stateRepository = stateRepository;
            _transactionReceiptRepository = transactionReceiptRepository;
        }
        

        public async Task<IEnumerable<BigInteger>> GetNonIndexedBlocksAsync(
            int take)
        {
            var nonIndexedBlocks = new List<BigInteger>(take);
            
            using (await _stateRepository.WaitLockAsync())
            {
                var indexationState = await _stateRepository.GetOrCreateAsync();
                
                // Update best block
                
                var bestBlockNumber = await _blockchainService.GetBestTrustedBlockNumberAsync();
                
                if (indexationState.TryToUpdateBestBlock(bestBlockNumber))
                {
                    await _stateRepository.UpdateAsync(indexationState);
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

                    await ReleaseBlockLockAsync(@lock.BlockNumber);
                }
                
                // Get non-indexed blocks
                foreach (var blockNumber in indexationState.GetNonIndexedBlockNumbers())
                {
                    if (blockLocks.All(x => x.BlockNumber != blockNumber))
                    {
                        nonIndexedBlocks.Add(blockNumber);

                        await _blockLockRepository.InsertOrReplaceAsync(blockNumber);
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

            return nonIndexedBlocks;
        }

        public async Task<IEnumerable<BigInteger>> IndexBlocksAsync(
            IEnumerable<BigInteger> blockNumbers)
        {
            var indexedBlocks = new ConcurrentBag<BigInteger>();
            var throttler = new SemaphoreSlim(_maxDegreeOfParallelism);

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
                        
                        throttler.Release();
                        
                    })
                
            ).ToList();

            await Task.WhenAll(indexationTasks);
            
            return indexedBlocks;
        }

        private Task<bool> IndexBlockAsync(
            BigInteger blockNumber)
        {
            throw new NotImplementedException();
        }
        
        public async Task MarkBlocksAsIndexed(
            IEnumerable<BigInteger> blockNumbers)
        {
            using (await _stateRepository.WaitLockAsync())
            {
                var indexationState = await _stateRepository.GetOrCreateAsync();
                var stateUpdateIsNecessary = false;
                
                foreach (var blockNumber in blockNumbers)
                {
                    if (indexationState.TryToMarkBlockAsIndexed(blockNumber))
                    {
                        stateUpdateIsNecessary = true;
                    }

                    await ReleaseBlockLockAsync(blockNumber);
                }

                if (stateUpdateIsNecessary)
                {
                    await _stateRepository.UpdateAsync(indexationState);
                }
            }
        }

        private async Task ReleaseBlockLockAsync(
            BigInteger blockNumber)
        {
            try
            {
                await _blockLockRepository.DeleteIfExistsAsync(blockNumber);
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to release indexation lock for block {blockNumber}.");
            }
        }
    }
}

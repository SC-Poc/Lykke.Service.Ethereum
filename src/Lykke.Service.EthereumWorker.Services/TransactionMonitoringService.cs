using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.Service.EthereumWorker.Core.Services;


namespace Lykke.Service.EthereumWorker.Services
{
    [UsedImplicitly]
    public class TransactionMonitoringService : ITransactionMonitoringService
    {
        private readonly IBlockchainService _blockchainService;
        private readonly ILog _log;
        private readonly ITransactionMonitoringTaskRepository _transactionMonitoringTaskRepository;
        private readonly ITransactionRepository _transactionRepository;

        
        public TransactionMonitoringService(
            IBlockchainService blockchainService,
            ILogFactory logFactory,
            ITransactionMonitoringTaskRepository transactionMonitoringTaskRepository,
            ITransactionRepository transactionRepository)
        {
            _blockchainService = blockchainService;
            _log = logFactory.CreateLog(this);
            _transactionMonitoringTaskRepository = transactionMonitoringTaskRepository;
            _transactionRepository = transactionRepository;
        }

        
        public async Task<bool> CheckAndUpdateStateAsync(
            Guid transactionId)
        {
            try
            {
                var transaction = await _transactionRepository.TryGetAsync(transactionId);
                
                if (transaction?.State == TransactionState.InProgress)
                {
                    var transactionResult = await _blockchainService.GetTransactionResultAsync(transaction.Hash);

                    if (transactionResult.IsCompleted)
                    {
                        if (!transactionResult.IsFailed)
                        {
                            transaction.OnSucceded
                            (
                                transactionResult.BlockNumber
                            );
                        }
                        else
                        {
                            transaction.OnFailed
                            (
                                transactionResult.BlockNumber,
                                transactionResult.Error
                            );
                        }

                        await _transactionRepository.UpdateAsync(transaction);
                    }
                    
                    LogTransactionResult(transactionId, transactionResult);

                    return transactionResult.IsCompleted;
                }
                else
                {
                    _log.Warning($"Transaction [{transactionId}] does not exist or has already been marked as completed.");

                    return true;
                }
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to check and update transaction [{transactionId}] state.");
                
                return false;
            }
        }

        public async Task CompleteMonitoringTaskAsync(
            string completionToken)
        {
            try
            {
                await _transactionMonitoringTaskRepository.CompleteAsync(completionToken);
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to complete transaction monitoring task.");
            }
        }

        public async Task<(TransactionMonitoringTask Task, string CompletionToken)> TryGetNextMonitoringTaskAsync()
        {
            try
            {
                return await _transactionMonitoringTaskRepository.TryGetAsync
                (
                    visibilityTimeout: TimeSpan.FromMinutes(1)
                );
            }
            catch (Exception e)
            {
                _log.Error(e, "Failed to get next transaction monitoring task.");

                return (null, null);
            }
        }

        private void LogTransactionResult(
            Guid transactionId,
            TransactionResult transactionResult)
        {
            if (transactionResult.IsCompleted)
            {
                _log.Info
                (
                    !transactionResult.IsFailed
                        ? $"Transaction [{transactionId}] succeded in block {transactionResult.BlockNumber}."
                        : $"Transaction [{transactionId}] failed in block {transactionResult.BlockNumber}."
                );
            }
            else
            {
                _log.Debug($"Transaction [{transactionId}] is in progress.");
            }
        }
    }
}

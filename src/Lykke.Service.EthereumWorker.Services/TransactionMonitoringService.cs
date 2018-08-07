using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.Service.EthereumWorker.Core.Services;

namespace Lykke.Service.EthereumWorker.Services
{
    [UsedImplicitly]
    public class TransactionMonitoringService : ITransactionMonitoringService
    {
        private readonly IBlockchainService _blockchainService;
        private readonly ITransactionMonitoringTaskRepository _transactionMonitoringTaskRepository;
        private readonly ITransactionRepository _transactionRepository;

        
        public TransactionMonitoringService(
            IBlockchainService blockchainService,
            ITransactionMonitoringTaskRepository transactionMonitoringTaskRepository,
            ITransactionRepository transactionRepository)
        {
            _blockchainService = blockchainService;
            _transactionMonitoringTaskRepository = transactionMonitoringTaskRepository;
            _transactionRepository = transactionRepository;
        }

        
        public async Task<bool> CheckAndUpdateStateAsync(
            Guid transactionId)
        {
            var transaction = await _transactionRepository.TryGetAsync(transactionId);
            var transactionCompleted = true;

            if (transaction?.State == TransactionState.InProgress)
            {
                throw new NotImplementedException();
            }

            return transactionCompleted;
        }

        public Task CompleteMonitoringTaskAsync(
            string completionToken)
        {
            return _transactionMonitoringTaskRepository.CompleteAsync(completionToken);
        }

        public Task<(TransactionMonitoringTask Task, string CompletionToken)> TryGetNextMonitoringTaskAsync()
        {
            return _transactionMonitoringTaskRepository.TryGetAsync
            (
                visibilityTimeout: TimeSpan.FromMinutes(1)
            );
        }
    }
}

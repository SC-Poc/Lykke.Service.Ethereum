using System;
using System.Threading.Tasks;
using Lykke.Service.EthereumCommon.Core.Domain;

namespace Lykke.Service.EthereumWorker.Core.Services
{
    public interface ITransactionMonitoringService
    {
        Task<bool> CheckAndUpdateStateAsync(
            Guid transactionId);
        
        Task CompleteMonitoringTaskAsync(
            string completionToken);
        
        Task<(TransactionMonitoringTask Task, string CompletionToken)> TryGetNextMonitoringTaskAsync();
    }
}

using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumWorker.Core.QueueConsumer;
using Lykke.Service.EthereumWorker.Core.Services;


namespace Lykke.Service.EthereumWorker.QueueConsumers
{
    [UsedImplicitly]
    public class TransactionMonitoringQueueConsumer : QueueConsumer<(TransactionMonitoringTask, string)>
    {
        private readonly ITransactionMonitoringService _transactionMonitoringService;
        
        
        public TransactionMonitoringQueueConsumer(
            int maxDegreeOfParallelism,
            ITransactionMonitoringService transactionMonitoringService)
        
            : base(maxDegreeOfParallelism: maxDegreeOfParallelism)
        {
            _transactionMonitoringService = transactionMonitoringService;
        }
        
        
        protected override async Task<(bool, (TransactionMonitoringTask, string))> TryGetNextTaskAsync()
        {
            var taskAndCompletionToken = await _transactionMonitoringService.TryGetNextMonitoringTaskAsync();

            return (taskAndCompletionToken.Task != null, taskAndCompletionToken);
        }

        protected override async Task ProcessTaskAsync(
            (TransactionMonitoringTask, string) taskAndCompletionToken)
        {
            var task = taskAndCompletionToken.Item1;
            var completionToken = taskAndCompletionToken.Item2;

            if (await _transactionMonitoringService.CheckAndUpdateStateAsync(task.TransactionId))
            {
                await _transactionMonitoringService.CompleteMonitoringTaskAsync(completionToken);
            }
        }
    }
}

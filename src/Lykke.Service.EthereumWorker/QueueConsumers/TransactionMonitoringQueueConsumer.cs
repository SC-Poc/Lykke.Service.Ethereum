using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumWorker.Core.QueueConsumer;
using Lykke.Service.EthereumWorker.Core.Services;


namespace Lykke.Service.EthereumWorker.QueueConsumers
{
    [UsedImplicitly]
    public class TransactionMonitoringQueueConsumer : QueueConsumer<(TransactionMonitoringTask Task, string CompletionToken)>
    {
        private readonly ILog _log;
        private readonly ITransactionMonitoringService _transactionMonitoringService;
        
        
        public TransactionMonitoringQueueConsumer(
            ILogFactory logFactory,
            Settings settings,
            ITransactionMonitoringService transactionMonitoringService)
        
            : base(maxDegreeOfParallelism: settings.MaxDegreeOfParallelism)
        {
            _log = logFactory.CreateLog(this);
            _transactionMonitoringService = transactionMonitoringService;
        }
        
        
        protected override async Task<(bool, (TransactionMonitoringTask, string))> TryGetNextTaskAsync()
        {
            var taskAndCompletionToken = await _transactionMonitoringService.TryGetNextMonitoringTaskAsync();

            return (taskAndCompletionToken.Task != null, taskAndCompletionToken);
        }

        protected override async Task ProcessTaskAsync(
            (TransactionMonitoringTask Task, string CompletionToken) taskAndCompletionToken)
        {
            var transactionChecked = await _transactionMonitoringService
                .CheckAndUpdateStateAsync(taskAndCompletionToken.Task.TransactionId);
            
            if (transactionChecked)
            {
                await _transactionMonitoringService.CompleteMonitoringTaskAsync(taskAndCompletionToken.CompletionToken);
            }
        }
        
        public override void Start()
        {
            _log.Info("Starting transaction monitoring...");
            
            base.Start();
            
            _log.Info("Transaction monitoring started.");
        }

        public override void Stop()
        {
            _log.Info("Stopping transaction monitoring...");
            
            base.Stop();
            
            _log.Info("Transaction monitoring stopped.");
        }
        
        
        public class Settings
        {
            public int MaxDegreeOfParallelism { get; set; }
        }
    }
}

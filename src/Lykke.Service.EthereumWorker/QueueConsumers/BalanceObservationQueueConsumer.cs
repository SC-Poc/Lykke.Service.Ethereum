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
    public sealed class BalanceObservationQueueConsumer : QueueConsumer<(BalanceObservationTask Task, string CompletionToken)>
    {
        private readonly IBalanceObservationService _balanceObservationService;
        private readonly ILog _log;
        
        
        public BalanceObservationQueueConsumer(
            IBalanceObservationService balanceObservationService,
            ILogFactory logFactory,
            Settings settings)
            
            : base(maxDegreeOfParallelism: settings.MaxDegreeOfParallelism)
        {
            _balanceObservationService = balanceObservationService;
            _log = logFactory.CreateLog(this);
        }

        protected override async Task<(bool, (BalanceObservationTask, string))> TryGetNextTaskAsync()
        {
            var taskAndCompletionToken = await _balanceObservationService.TryGetNextObservationTaskAsync();

            return (taskAndCompletionToken.Task != null, taskAndCompletionToken);
        }

        protected override async Task ProcessTaskAsync(
            (BalanceObservationTask Task, string CompletionToken) taskAndCompletionToken)
        {
            var balanceChecked = await _balanceObservationService
                .CheckAndUpdateBalanceAsync
                (
                    taskAndCompletionToken.Task.Address,
                    taskAndCompletionToken.Task.BlockNumber
                );

            if (balanceChecked)
            {
                await _balanceObservationService
                    .CompleteObservationTaskAsync(taskAndCompletionToken.CompletionToken);
            }
        }

        public override void Start()
        {
            _log.Info("Starting balances observation...");
            
            base.Start();
            
            _log.Info("Balances observation started.");
        }

        public override void Stop()
        {
            _log.Info("Stopping balances observation...");
            
            base.Stop();
            
            _log.Info("Balances observation stopped.");
        }


        public class Settings
        {
            public int MaxDegreeOfParallelism { get; set; }
        }
    }
}

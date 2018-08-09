using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.Service.EthereumWorker.Core.QueueConsumer;
using Lykke.Service.EthereumWorker.Core.Services;


namespace Lykke.Service.EthereumWorker.QueueConsumers
{
    [UsedImplicitly]
    public sealed class BalanceObservationQueueConsumer : QueueConsumer<(BalanceObservationTask, string)>
    {
        private readonly IBalanceObservationService _balanceObservationService;
        
        
        public BalanceObservationQueueConsumer(
            IBalanceObservationService balanceObservationService,
            Settings settings)
            
            : base(maxDegreeOfParallelism: settings.MaxDegreeOfParallelism)
        {
            _balanceObservationService = balanceObservationService;
        }

        protected override async Task<(bool, (BalanceObservationTask, string))> TryGetNextTaskAsync()
        {
            var taskAndCompletionToken = await _balanceObservationService.TryGetNextObseravtionTaskAsync();

            return (taskAndCompletionToken.Task != null, taskAndCompletionToken);
        }

        protected override async Task ProcessTaskAsync(
            (BalanceObservationTask, string) taskAndCompletionToken)
        {
            var task = taskAndCompletionToken.Item1;
            var completionToken = taskAndCompletionToken.Item2;
            
            await _balanceObservationService.CheckAndUpdateBalanceAsync(task.Address);

            await _balanceObservationService.CompleteObservationTaskAsync(completionToken);
        }


        public class Settings
        {
            public int MaxDegreeOfParallelism { get; set; }
        }
    }
}

using System.Threading.Tasks;
using Lykke.Service.EthereumWorker.Core.Domain;

namespace Lykke.Service.EthereumWorker.Core.Services
{
    public interface IBalanceObservationService
    {
        Task<(BalanceObservationTask Task, string CompletionToken)> TryGetNextObseravtionTaskAsync();

        Task CheckAndUpdateBalanceAsync(
            string address);

        Task CompleteObservationTaskAsync(
            string completionToken);
    }
}

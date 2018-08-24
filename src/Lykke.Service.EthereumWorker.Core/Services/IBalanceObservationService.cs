using System.Numerics;
using System.Threading.Tasks;
using Lykke.Service.EthereumCommon.Core.Domain;


namespace Lykke.Service.EthereumWorker.Core.Services
{
    public interface IBalanceObservationService
    {
        Task<(BalanceObservationTask Task, string CompletionToken)> TryGetNextObseravtionTaskAsync();

        Task<bool> CheckAndUpdateBalanceAsync(
            string address,
            BigInteger blockNumber);

        Task CompleteObservationTaskAsync(
            string completionToken);
    }
}

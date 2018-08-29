using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Domain;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface IBalanceService
    {
        Task<bool> BeginObservationIfNotObservingAsync(
            [NotNull] string address);
        
        Task<bool> EndObservationIfObservingAsync(
            [NotNull] string address);

        Task<(IEnumerable<Balance> Balances, string ContinuationToken)> GetTransferableBalancesAsync(
            int take,
            [NotNull] string continuationToken);
    }
}

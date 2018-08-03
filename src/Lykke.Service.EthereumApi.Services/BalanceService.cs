using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;

namespace Lykke.Service.EthereumApi.Services
{
    [UsedImplicitly]
    public class BalanceService : IBalanceService
    {
        private readonly IObservableBalanceRepository _observableAccountStateRepository;

        
        public BalanceService(
            IObservableBalanceRepository observableAccountStateRepository)
        {
            _observableAccountStateRepository = observableAccountStateRepository;
        }


        public Task<bool> BeginObservationIfNotObservingAsync(
            string address)
        {
            return _observableAccountStateRepository
                .CreateIfNotExistsAsync(address);
        }

        public Task<bool> EndObservationIfObservingAsync(
            string address)
        {
            return _observableAccountStateRepository
                .DeleteIfExistsAsync(address);
        }

        public Task<(IEnumerable<Balance> Balances, string ContinuationToken)> GetTransferableBalancesAsync(
            int take, 
            string continuationToken)
        {
            return _observableAccountStateRepository
                .GetAllTransferableBalancesAsync(take, continuationToken);
        }
    }
}

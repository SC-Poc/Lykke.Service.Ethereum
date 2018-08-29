using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;

namespace Lykke.Service.EthereumApi.Services
{
    [UsedImplicitly]
    public class BalanceService : IBalanceService
    {
        private readonly IBalanceObservationTaskRepository _balanceObservationTaskRepository;
        private readonly ILog _log;
        private readonly IObservableBalanceRepository _observableAccountStateRepository;
        
        
        public BalanceService(
            IBalanceObservationTaskRepository balanceObservationTaskRepository,
            ILogFactory logFactory,
            IObservableBalanceRepository observableAccountStateRepository)
        {
            _balanceObservationTaskRepository = balanceObservationTaskRepository;
            _log = logFactory.CreateLog(this);
            _observableAccountStateRepository = observableAccountStateRepository;
        }


        public async Task<bool> BeginObservationIfNotObservingAsync(
            string address)
        {
            var observationBegan = await _observableAccountStateRepository
                .CreateIfNotExistsAsync(address);

            if (observationBegan)
            {
                _log.Info($"Balance observation for wallet [{address}] started.");
            }
            
            await _balanceObservationTaskRepository.EnqueueAsync(new BalanceObservationTask
            {
                Address = address
            });

            return observationBegan;
        }

        public async Task<bool> EndObservationIfObservingAsync(
            string address)
        {
            var observationEnded = await  _observableAccountStateRepository
                .DeleteIfExistsAsync(address);
            
            if (observationEnded)
            {
                _log.Info($"Balance observation for wallet [{address}] stoppped.");
            }

            return observationEnded;
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

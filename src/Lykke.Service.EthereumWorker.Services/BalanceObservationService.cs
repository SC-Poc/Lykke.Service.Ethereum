using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.Service.EthereumWorker.Core.Repositories;
using Lykke.Service.EthereumWorker.Core.Services;


namespace Lykke.Service.EthereumWorker.Services
{
    [UsedImplicitly]
    public class BalanceObservationService : IBalanceObservationService
    {
        private readonly IBalanceObservationTaskRepository _balanceObservationTaskRepository;
        private readonly IBlockchainService _blockchainService;
        private readonly IObservableBalanceRepository _observableBalanceRepository;


        public BalanceObservationService(
            IBalanceObservationTaskRepository balanceObservationTaskRepository,
            IBlockchainService blockchainService,
            IObservableBalanceRepository observableBalanceRepository)
        {
            _balanceObservationTaskRepository = balanceObservationTaskRepository;
            _blockchainService = blockchainService;
            _observableBalanceRepository = observableBalanceRepository;
        }


        public async Task CheckAndUpdateBalanceAsync(
            string address)
        {
            var bestTrustedBlockNumber = await _blockchainService.GetBestTrustedBlockNumberAsync();
            var currentBalance = await _observableBalanceRepository.TryGetAsync(address);

            if (currentBalance != null && currentBalance.BlockNumber < bestTrustedBlockNumber)
            {
                var balance = await _blockchainService.GetBalanceAsync(address, bestTrustedBlockNumber);
                
                currentBalance.Amount = balance;
                currentBalance.BlockNumber = bestTrustedBlockNumber;

                await _observableBalanceRepository.UpdateSafelyAsync(currentBalance);
            }
        }

        public Task CompleteObservationTaskAsync(
            string completionToken)
        {
            return _balanceObservationTaskRepository.CompleteAsync(completionToken);
        }

        public Task<(BalanceObservationTask Task, string CompletionToken)> TryGetNextObseravtionTaskAsync()
        {
            return _balanceObservationTaskRepository.TryGetAsync(TimeSpan.FromMinutes(1));
        }
    }
}

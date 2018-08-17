using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Core.Domain;
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
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly IObservableBalanceRepository _observableBalanceRepository;


        public BalanceObservationService(
            IBalanceObservationTaskRepository balanceObservationTaskRepository,
            IBlockchainService blockchainService,
            IChaosKitty chaosKitty,
            ILogFactory logFactory,
            IObservableBalanceRepository observableBalanceRepository)
        {
            _balanceObservationTaskRepository = balanceObservationTaskRepository;
            _blockchainService = blockchainService;
            _chaosKitty = chaosKitty;
            _log = logFactory.CreateLog(this);
            _observableBalanceRepository = observableBalanceRepository;
        }


        public async Task<bool> CheckAndUpdateBalanceAsync(
            string address)
        {
            try
            {
                var bestTrustedBlockNumber = await _blockchainService.GetBestTrustedBlockNumberAsync();
                var currentBalance = await _observableBalanceRepository.TryGetAsync(address);

                if (currentBalance != null && currentBalance.BlockNumber < bestTrustedBlockNumber)
                {
                    var balance = await _blockchainService.GetBalanceAsync(address, bestTrustedBlockNumber);
                
                    currentBalance.Amount = balance;
                    currentBalance.BlockNumber = bestTrustedBlockNumber;

                    _chaosKitty.Meow(address);    
                    
                    await _observableBalanceRepository.UpdateSafelyAsync(currentBalance);
                    
                    _log.Info($"Account [{address}] balance updated to [{balance} {Constants.AssetId}] at block [{bestTrustedBlockNumber}].");
                }
                else
                {
                    _log.Debug($"Account [{address}] balance is not observable or has already been updated.");
                }

                return true;
            }
            catch (Exception e)
            {
                _log.Error(e, $"Failed to check balance of account [{address}].");

                return false;
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

using System;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Domain;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;

namespace Lykke.Service.EthereumApi.Services
{
    [UsedImplicitly]
    public class TransactionService : ITransactionService
    {
        private readonly IBlockchainService _blockchainService;
        private readonly BigInteger _minimalTransactionAmount;
        private readonly ITransactionMonitoringTaskRepository _tranferTransactionMonitoringTaskRepository;
        private readonly ITransactionRepository _transactionRepository;

        
        public TransactionService(
            IBlockchainService blockchainService,
            ITransactionMonitoringTaskRepository tranferTransactionMonitoringTaskRepository,
            ITransactionRepository transactionRepository,
            Settings settings)
        {
            _blockchainService = blockchainService;
            _minimalTransactionAmount = settings.MinimalTransactionAmount;
            _tranferTransactionMonitoringTaskRepository = tranferTransactionMonitoringTaskRepository;
            _transactionRepository = transactionRepository;
        }

        
        public async Task<BuildTransactionResult> BuildTransactionAsync(
            Guid transactionId,
            string from,
            string to,
            BigInteger amount)
        {
            if (amount < _minimalTransactionAmount)
            {
                return BuildTransactionResult.AmountIsTooSmall;
            }
            
            var transaction = await _transactionRepository.TryGetAsync(transactionId);

            if (transaction == null)
            {
                var balance = await _blockchainService.GetBalanceAsync(from);

                if (balance < amount)
                {
                    return BuildTransactionResult.BalanceIsNotEnough;
                }

                transaction = Transaction.Build
                (
                    transactionId: transactionId,
                    from: from,
                    to: to,
                    amount: amount,
                    data: await _blockchainService.BuildTransactionAsync(from, to, amount)
                );

                await _transactionRepository.AddAsync(transaction);

                return BuildTransactionResult.Success(transaction.Data);
            }
            else
            {
                switch (transaction.State)
                {
                    case TransactionState.Built:
                        return BuildTransactionResult.Success(transaction.Data);
                    
                    case TransactionState.InProgress:
                    case TransactionState.Completed:
                    case TransactionState.Failed:
                        return BuildTransactionResult.TransactionHasBeenBroadcasted;
                    
                    case TransactionState.Deleted:
                        return BuildTransactionResult.TransactionHasBeenDeleted;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public async Task<BroadcastTransactionResult> BroadcastTransactionAsync(
            Guid transactionId,
            string signedTxData)
        {
            var transaction = await _transactionRepository.TryGetAsync(transactionId);

            if (transaction != null)
            {
                if (transaction.Amount < _minimalTransactionAmount)
                {
                    return BroadcastTransactionResult.AmountIsTooSmall;
                }

                var balance = await _blockchainService.GetBalanceAsync(transaction.From);

                if (balance < transaction.Amount)
                {
                    return BroadcastTransactionResult.BalanceIsNotEnough;
                }
                
                switch (transaction.State)
                {
                    case TransactionState.Built:

                        var txHash = await _blockchainService.BroadcastTransactionAsync(signedTxData);
                        
                        transaction.OnBroadcasted
                        (
                            hash: txHash,
                            signedData: signedTxData
                        );
                        
                        await _transactionRepository.UpdateAsync(transaction);
                        
                        await _tranferTransactionMonitoringTaskRepository.EnqueueAsync
                        (
                            new TransactionMonitoringTask
                            {
                                TransactionId = transactionId
                            }
                        );
                        
                        return BroadcastTransactionResult.Success(txHash);
                    
                    case TransactionState.InProgress:
                    case TransactionState.Completed:
                    case TransactionState.Failed:
                        return BroadcastTransactionResult.TransactionHasBeenBroadcasted;
                    
                    case TransactionState.Deleted:
                        return BroadcastTransactionResult.TransactionHasBeenDeleted;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                return BroadcastTransactionResult.OperationHasNotBeenFound;
            }
        }

        public async Task<bool> DeleteTransactionIfExistsAsync(
            Guid transactionId)
        {
            var transaction = await _transactionRepository.TryGetAsync(transactionId);

            if (transaction != null)
            {
                transaction.OnDeleted();

                await _transactionRepository.UpdateAsync(transaction);

                return true;
            }
            else
            {
                return false;
            }
        }

        public Task<Transaction> TryGetTransactionAsync(
            Guid transactionId)
        {
            return _transactionRepository.TryGetAsync(transactionId);
        }


        public class Settings
        {
            public BigInteger MinimalTransactionAmount { get; set; }
        }
    }
}

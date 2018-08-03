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
            BigInteger minimalTransactionAmount,
            ITransactionMonitoringTaskRepository tranferTransactionMonitoringTaskRepository,
            ITransactionRepository transactionRepository)
        {
            _blockchainService = blockchainService;
            _minimalTransactionAmount = minimalTransactionAmount;
            _tranferTransactionMonitoringTaskRepository = tranferTransactionMonitoringTaskRepository;
            _transactionRepository = transactionRepository;
        }

        
        public async Task<BuildTransactionResult> BuildTransactionAsync(
            Guid operationId,
            string from,
            string to,
            BigInteger amount)
        {
            if (amount < _minimalTransactionAmount)
            {
                return BuildTransactionResult.AmountIsTooSmall;
            }
            
            var transaction = await _transactionRepository.TryGetAsync(operationId);

            if (transaction == null)
            {
                var balance = await _blockchainService.GetBalanceAsync(from);

                if (balance < amount)
                {
                    return BuildTransactionResult.BalanceIsNotEnough;
                }

                transaction = Transaction.Build
                (
                    operationId: operationId,
                    from: from,
                    to: to,
                    amount: amount,
                    data: await _blockchainService.BuildTransactionAsync(to, amount)
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
            Guid operationId,
            string signedTxData)
        {
            var transaction = await _transactionRepository.TryGetAsync(operationId);

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
                                OperationId = operationId
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
                return BroadcastTransactionResult.OperationHasNotBeenFoun;
            }
        }

        public async Task<bool> DeleteTransactionIfExistsAsync(
            Guid operationId)
        {
            var transaction = await _transactionRepository.TryGetAsync(operationId);

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
            Guid operationId)
        {
            return _transactionRepository.TryGetAsync(operationId);
        }
    }
}

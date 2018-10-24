using JetBrains.Annotations;

namespace Lykke.Service.EthereumApi.Core.Domain
{
    [PublicAPI]
    public abstract class BuildTransactionResult
    {
        public static readonly Error AmountIsTooSmall 
            = new Error(BuildTransactionError.AmountIsTooSmall);
        
        public static readonly Error BalanceIsNotEnough 
            = new Error(BuildTransactionError.BalanceIsNotEnough);
        
        public static readonly Error GasAmountIsTooHigh 
            = new Error(BuildTransactionError.GasAmountIsTooHigh);
        
        public static readonly Error TransactionHasBeenBroadcasted 
            = new Error(BuildTransactionError.TransactionHasBeenDeleted);
        
        public static readonly Error TransactionHasBeenDeleted 
            = new Error(BuildTransactionError.TransactionHasBeenDeleted);
        
        public static TransactionContext Success(string txData)
            => new TransactionContext(txData);
        
        
        public class TransactionContext : BuildTransactionResult
        {
            internal TransactionContext(
                string txData)
            {
                String = txData;
            }

            public string String { get; }
        }

        public class Error : BuildTransactionResult
        {
            internal Error(
                BuildTransactionError type)
            {
                Type = type;
            }
            
            public BuildTransactionError Type { get; }
        }
    }
}

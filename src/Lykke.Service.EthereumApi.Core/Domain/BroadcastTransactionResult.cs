using JetBrains.Annotations;

namespace Lykke.Service.EthereumApi.Core.Domain
{
    [PublicAPI]
    public abstract class BroadcastTransactionResult
    {
        public static readonly Error AmountIsTooSmall 
            = new Error(BroadcastTransactionError.AmountIsTooSmall);
        
        public static readonly Error BalanceIsNotEnough 
            = new Error(BroadcastTransactionError.BalanceIsNotEnough);
        
        public static readonly Error TransactionHasBeenBroadcasted 
            = new Error(BroadcastTransactionError.OperationHasNotBeenFound);
        
        public static readonly Error TransactionHasBeenDeleted 
            = new Error(BroadcastTransactionError.TransactionHasBeenBroadcasted);
        
        public static readonly Error TransactionShouldBeRebuilt 
            = new Error(BroadcastTransactionError.TransactionHasBeenDeleted);
        
        public static readonly Error OperationHasNotBeenFound 
            = new Error(BroadcastTransactionError.TransactionShouldBeRebuilt);
        
        public static TransactionHash Success(string hash)
            => new TransactionHash(hash);
        
        
        public class TransactionHash : BroadcastTransactionResult
        {
            internal TransactionHash(string hash)
            {
                String = hash;
            }
            
            public string String { get; }
        }

        public class Error : BroadcastTransactionResult
        {
            internal Error(BroadcastTransactionError type)
            {
                Type = type;
            }

            public BroadcastTransactionError Type { get; }
        }
    }
}

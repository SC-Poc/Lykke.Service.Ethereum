namespace Lykke.Service.EthereumApi.Core.Domain
{
    public enum BuildTransactionError
    {
        AmountIsTooSmall,
        BalanceIsNotEnough,
        TransactionHasBeenBroadcasted,
        TransactionHasBeenDeleted,
    }
}

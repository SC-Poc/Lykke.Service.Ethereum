namespace Lykke.Service.EthereumApi.Core.Domain
{
    public enum BroadcastTransactionError
    {
        AmountIsTooSmall,
        BalanceIsNotEnough,
        TransactionHasBeenBroadcasted,
        TransactionHasBeenDeleted,
        TransactionShouldBeRebuilt,
        OperationHasNotBeenFound
    }
}

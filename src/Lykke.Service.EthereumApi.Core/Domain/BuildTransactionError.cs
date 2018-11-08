namespace Lykke.Service.EthereumApi.Core.Domain
{
    public enum BuildTransactionError
    {
        AmountIsTooSmall,
        BalanceIsNotEnough,
        GasAmountIsTooHigh,
        TargetAddressBlacklistedOrInvalid,
        TransactionHasBeenBroadcasted,
        TransactionHasBeenDeleted,
    }
}

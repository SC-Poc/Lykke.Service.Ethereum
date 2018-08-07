using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.EthereumApi.Validation
{
    [UsedImplicitly]
    public class BroadcastTransactionRequestValidator : AbstractValidator<BroadcastTransactionRequest>
    {
        public BroadcastTransactionRequestValidator()
        {
            RuleFor(x => x.OperationId)
                .TransactionIdMustBeNonEmptyGuid();
            
            RuleFor(x => x.SignedTransaction)
                .MustBeHexString();
        }
    }
}

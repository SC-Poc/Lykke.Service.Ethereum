using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Models;

namespace Lykke.Service.EthereumApi.Validation
{
    [UsedImplicitly]
    public class OperationRequestValidator : AbstractValidator<TransactionRequest>
    {
        public OperationRequestValidator()
        {
            RuleFor(x => x.TransactionId)
                .TransactionIdMustBeNonEmptyGuid();
        }
    }
}

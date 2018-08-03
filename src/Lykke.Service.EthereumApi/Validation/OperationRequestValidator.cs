using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Models;

namespace Lykke.Service.EthereumApi.Validation
{
    [UsedImplicitly]
    public class OperationRequestValidator : AbstractValidator<OperationRequest>
    {
        public OperationRequestValidator()
        {
            RuleFor(x => x.OperationId)
                .OperationIdMustBeNonEmptyGuid();
        }
    }
}

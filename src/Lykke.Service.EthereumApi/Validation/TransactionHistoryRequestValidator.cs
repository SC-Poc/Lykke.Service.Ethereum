using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Models;

namespace Lykke.Service.EthereumApi.Validation
{
    [UsedImplicitly]
    public class TransactionHistoryRequestValidator : AbstractValidator<TransactionHistoryRequest>
    {
        public TransactionHistoryRequestValidator()
        {
            RuleFor(x => x.Address)
                .AddressMustBeValid();

            RuleFor(x => x.Take)
                .GreaterThan(1);
        }
    }
}

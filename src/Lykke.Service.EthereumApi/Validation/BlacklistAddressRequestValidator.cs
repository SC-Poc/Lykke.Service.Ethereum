using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Models;

namespace Lykke.Service.EthereumApi.Validation
{
    [UsedImplicitly]
    public class BlacklistAddressRequestValidator : AbstractValidator<BlacklistAddressRequest>
    {
        public BlacklistAddressRequestValidator()
        {
            RuleFor(x => x.Address)
                .AddressMustBeValid();

            RuleFor(x => x.BlacklistingReason)
                .NotEmpty()
                .MaximumLength(255);
        }
    }
}

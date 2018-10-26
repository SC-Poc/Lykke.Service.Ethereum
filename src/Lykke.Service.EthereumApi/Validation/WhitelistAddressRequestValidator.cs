using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Models;

namespace Lykke.Service.EthereumApi.Validation
{
    [UsedImplicitly]
    public class WhitelistAddressRequestValidator : AbstractValidator<WhitelistAddressRequest>
    {
        public WhitelistAddressRequestValidator()
        {
            RuleFor(x => x.Address)
                .AddressMustBeValid();
            
            RuleFor(x => x.MaxGasAmount)
                .AmountMustBeValid();
        }
    }
}

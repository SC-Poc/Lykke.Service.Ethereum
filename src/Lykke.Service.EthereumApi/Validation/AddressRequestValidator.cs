using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Models;

namespace Lykke.Service.EthereumApi.Validation
{
    [UsedImplicitly]
    public class AddressRequestValidator : AbstractValidator<AddressRequest>
    {
        public AddressRequestValidator()
        {
            RuleFor(x => x.Address)
                .AddressMustBeValid();
        }
    }
}

using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Models;

namespace Lykke.Service.EthereumApi.Validation
{
    [UsedImplicitly]
    public class AssetRequestValidator : AbstractValidator<AssetRequest>
    {
        public AssetRequestValidator()
        {
            RuleFor(x => x.AssetId)
                .AssetMustBeSupported();
        }
    }
}

using JetBrains.Annotations;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface IAddressService
    {
        bool Validate(
            [NotNull] string address);
    }
}

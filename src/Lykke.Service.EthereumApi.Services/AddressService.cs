using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumCommon.Core;

namespace Lykke.Service.EthereumApi.Services
{
    [UsedImplicitly]
    public class AddressService : IAddressService
    {
        public bool Validate(
            string address)
        {
            return Address.ValidateFormatAndChecksum(address);
        }
    }
}

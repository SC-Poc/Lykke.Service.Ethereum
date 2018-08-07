using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumCommon.Core;

namespace Lykke.Service.EthereumApi.Services
{
    [UsedImplicitly]
    public class AddressService : IAddressService
    {
        
        public async Task<bool> ValidateAsync(
            string address)
        {
            // TODO: Ensure that address does not elong to contract
            
            return Address.ValidateFormatAndChecksum(address);
        }
    }
}

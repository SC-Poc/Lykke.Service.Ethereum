using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumCommon.Core;

namespace Lykke.Service.EthereumApi.Services
{
    [UsedImplicitly]
    public class AddressService : IAddressService
    {
        private readonly IBlockchainService _blockchainService;

        
        public AddressService(
            IBlockchainService blockchainService)
        {
            _blockchainService = blockchainService;
        }

        
        public async Task<bool> ValidateAsync(
            string address)
        {
            return Address.ValidateFormatAndChecksum(address)
                && await _blockchainService.IsWalletAsync(address);
        }
    }
}

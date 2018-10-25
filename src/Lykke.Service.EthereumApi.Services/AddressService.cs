using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Domain;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;


namespace Lykke.Service.EthereumApi.Services
{
    [UsedImplicitly]
    public class AddressService : IAddressService
    {
        private readonly IBlacklistedAddressRepository _blacklistedAddressRepository;
        private readonly IWhitelistedAddressRepository _whitelistedAddressRepository;
        private readonly IBlockchainService _blockchainService;

        
        public AddressService(
            IBlacklistedAddressRepository blacklistedAddressRepository,
            IWhitelistedAddressRepository whitelistedAddressRepository,
            IBlockchainService blockchainService)
        {
            _blacklistedAddressRepository = blacklistedAddressRepository;
            _whitelistedAddressRepository = whitelistedAddressRepository;
            _blockchainService = blockchainService;
        }


        public async Task<AddAddressResult> AddAddressToBlacklistAsync(
            string address,
            string reason)
        {
            if (await _blacklistedAddressRepository.AddIfNotExistsAsync(address, reason))
            {
                return AddAddressResult.Success;
            }
            else
            {
                
                return AddAddressResult.HasAlreadyBeenAdded;
            }
        }
        
        public async Task<AddAddressResult> AddAddressToWhitelistAsync(
            string address)
        {
            if (await _whitelistedAddressRepository.AddIfNotExistsAsync(address))
            {
                return AddAddressResult.Success;
            }
            else
            {
                return AddAddressResult.HasAlreadyBeenAdded;
            }
        }

        public Task<(IEnumerable<BlacklistedAddress> BlacklistedAddresses, string ContinuationToken)> GetBlacklistedAddressesAsync(
            int take,
            string continuationToken)
        {
            return _blacklistedAddressRepository.GetAllAsync(take, continuationToken);
        }

        public Task<(IEnumerable<string> WhitelistedAddresses, string ContinuationToken)> GetWhitelistedAddressesAsync(
            int take,
            string continuationToken)
        {
            return _whitelistedAddressRepository.GetAllAsync(take, continuationToken);
        }

        public async Task<RemoveAddressResult> RemoveAddressFromBlacklistAsync(
            string address)
        {
            if (await _blacklistedAddressRepository.RemoveIfExistsAsync(address))
            {
                return RemoveAddressResult.Success;
            }
            else
            {
                return RemoveAddressResult.NotFound;
            }
        }

        public async Task<RemoveAddressResult> RemoveAddressFromWhitelistAsync(
            string address)
        {
            if (await _whitelistedAddressRepository.RemoveIfExistsAsync(address))
            {
                return RemoveAddressResult.Success;
            }
            else
            {
                return RemoveAddressResult.NotFound;
            }
        }

        public async Task<string> TryGetBlacklistingReason(
            string address)
        {
            var blacklistedAddress = await _blacklistedAddressRepository.TryGetAsync(address);

            if (blacklistedAddress != null)
            {
                var reason = blacklistedAddress.BlacklistingReason;

                return !string.IsNullOrEmpty(reason) ? reason : "Unspecified";
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> ValidateAsync(
            string address)
        {
            if (Address.ValidateFormatAndChecksum(address, true, true))
            {
                return await _blockchainService.IsWalletAsync(address)
                   ||  await _whitelistedAddressRepository.ContainsAsync(address)
                   || !await _blacklistedAddressRepository.ContainsAsync(address);
            }
            else
            {
                return false;
            }
        }
    }
}

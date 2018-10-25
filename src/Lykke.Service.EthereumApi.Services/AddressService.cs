using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
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
        private readonly ILog _log;
        
        
        public AddressService(
            IBlacklistedAddressRepository blacklistedAddressRepository,
            IBlockchainService blockchainService,
            IWhitelistedAddressRepository whitelistedAddressRepository,
            ILogFactory logFactory)
        {
            _blacklistedAddressRepository = blacklistedAddressRepository;
            _blockchainService = blockchainService;
            _whitelistedAddressRepository = whitelistedAddressRepository;
            _log = logFactory.CreateLog(this);
        }


        public async Task<AddAddressResult> AddAddressToBlacklistAsync(
            string address,
            string reason)
        {
            if (await _blacklistedAddressRepository.AddIfNotExistsAsync(address, reason))
            {
                _log.Info($"Address [{address}] has been blacklisted.");
                
                return AddAddressResult.Success;
            }
            else
            {
                return AddAddressResult.HasAlreadyBeenAdded;
            }
        }

        public async Task<AddAddressResult> AddAddressToWhitelistAsync(
            string address,
            BigInteger maxGasAmount)
        {
            if (await _whitelistedAddressRepository.AddIfNotExistsAsync(address, maxGasAmount))
            {
                _log.Info($"Address [{address}] has been whitelisted.");
                
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

        Task<(IEnumerable<WhitelistedAddress> WhitelistedAddresses, string ContinuationToken)> IAddressService.GetWhitelistedAddressesAsync(
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
                _log.Info($"Address [{address}] has been removed from blacklist.");
                
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
                _log.Info($"Address [{address}] has been removed from whitelist.");
                
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

        public async Task<BigInteger?> TryGetCustomMaxGasAmountAsync(
            string address)
        {
            var whitelistedAddress = await _whitelistedAddressRepository.TryGetAsync(address);

            return whitelistedAddress?.MaxGasAmount;
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

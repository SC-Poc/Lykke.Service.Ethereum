using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Domain;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface IAddressService
    {
        Task<AddAddressResult> AddAddressToBlacklistAsync(
            string address,
            string reason);

        Task<AddAddressResult> AddAddressToWhitelistAsync(
            string address);

        Task<(IEnumerable<BlacklistedAddress> BlacklistedAddresses, string ContinuationToken)> GetBlacklistedAddressesAsync(
            int take,
            string continuationToken);
        
        Task<(IEnumerable<string> WhitelistedAddresses, string ContinuationToken)> GetWhitelistedAddressesAsync(
            int take,
            string continuationToken);

        Task<RemoveAddressResult> RemoveAddressFromBlacklistAsync(
            string address);

        Task<RemoveAddressResult> RemoveAddressFromWhitelistAsync(
            string address);

        Task<string> TryGetBlacklistingReason(
            string address);
        
        Task<bool> ValidateAsync(
            [NotNull] string address);
    }
}

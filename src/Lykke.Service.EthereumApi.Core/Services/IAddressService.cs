using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Domain;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface IAddressService
    {
        Task<AddAddressResult> AddAddressToBlacklistAsync(
            [NotNull] string address,
            [NotNull] string reason);

        Task<AddAddressResult> AddAddressToWhitelistAsync(
            [NotNull] string address,
            BigInteger maxGasAmount);

        Task<(IEnumerable<BlacklistedAddress> BlacklistedAddresses, string ContinuationToken)> GetBlacklistedAddressesAsync(
            int take,
            [CanBeNull] string continuationToken);
        
        Task<(IEnumerable<WhitelistedAddress> WhitelistedAddresses, string ContinuationToken)> GetWhitelistedAddressesAsync(
            int take,
            [CanBeNull] string continuationToken);

        Task<RemoveAddressResult> RemoveAddressFromBlacklistAsync(
            [NotNull] string address);

        Task<RemoveAddressResult> RemoveAddressFromWhitelistAsync(
            [NotNull] string address);

        [ItemCanBeNull]
        Task<string> TryGetBlacklistingReason(
            [NotNull] string address);

        [ItemCanBeNull]
        Task<BigInteger?> TryGetCustomMaxGasAmountAsync(
            [NotNull] string address);

        Task<bool> ValidateAsync(
            [NotNull] string address);
    }
}

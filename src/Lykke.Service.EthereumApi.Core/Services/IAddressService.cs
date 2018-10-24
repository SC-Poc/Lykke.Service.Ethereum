using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Domain;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface IAddressService
    {
        Task<AddAddressResult> AddAddressToBlacklistAsync(
            string address,
            string reason);

        Task<AddAddressResult> AddAddressToWhitelistAsync(
            string address);

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

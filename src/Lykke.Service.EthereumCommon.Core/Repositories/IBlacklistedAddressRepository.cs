using System.Threading.Tasks;
using Lykke.Service.EthereumCommon.Core.Domain;

namespace Lykke.Service.EthereumCommon.Core.Repositories
{
    public interface IBlacklistedAddressRepository
    {
        Task<bool> AddIfNotExistsAsync(
            string address,
            string reason);

        Task<bool> ContainsAsync(
            string address);

        Task<BlacklistedAddress> TryGetAsync(
            string address);
        
        Task<bool> RemoveIfExistsAsync(
            string address);
    }
}

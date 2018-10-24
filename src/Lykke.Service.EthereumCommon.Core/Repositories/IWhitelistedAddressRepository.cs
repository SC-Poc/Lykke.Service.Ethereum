using System.Threading.Tasks;

namespace Lykke.Service.EthereumCommon.Core.Repositories
{
    public interface IWhitelistedAddressRepository
    {
        Task<bool> AddIfNotExistsAsync(
            string address);
        
        Task<bool> ContainsAsync(
            string address);
        
        Task<bool> RemoveIfExistsAsync(
            string address);
    }
}

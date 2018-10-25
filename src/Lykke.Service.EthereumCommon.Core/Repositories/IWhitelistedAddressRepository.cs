using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.EthereumCommon.Core.Repositories
{
    public interface IWhitelistedAddressRepository
    {
        Task<bool> AddIfNotExistsAsync(
            string address);
        
        Task<bool> ContainsAsync(
            string address);
        
        Task<(IEnumerable<string> Addresses, string ContinuationToken)> GetAllAsync(
            int take,
            string continuationToken);
        
        Task<bool> RemoveIfExistsAsync(
            string address);
    }
}

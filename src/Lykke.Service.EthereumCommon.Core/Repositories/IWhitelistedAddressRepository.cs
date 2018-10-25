using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Service.EthereumCommon.Core.Domain;

namespace Lykke.Service.EthereumCommon.Core.Repositories
{
    public interface IWhitelistedAddressRepository
    {
        Task<bool> AddIfNotExistsAsync(
            string address,
            BigInteger maxGasAmount);

        Task<bool> ContainsAsync(
            string address);
        
        Task<(IEnumerable<WhitelistedAddress> Addresses, string ContinuationToken)> GetAllAsync(
            int take,
            string continuationToken);
        
        Task<bool> RemoveIfExistsAsync(
            string address);

        Task<WhitelistedAddress> TryGetAsync(
            string address);
    }
}

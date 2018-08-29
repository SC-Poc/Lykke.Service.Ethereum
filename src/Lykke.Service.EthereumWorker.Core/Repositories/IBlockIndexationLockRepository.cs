using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Service.EthereumWorker.Core.Domain;

namespace Lykke.Service.EthereumWorker.Core.Repositories
{
    public interface IBlockIndexationLockRepository
    {
        Task DeleteIfExistsAsync(
            BigInteger blockNumber);
        
        Task<IEnumerable<BlockIndexationLock>> GetAsync();
        
        Task InsertOrReplaceAsync(
            BigInteger blockNumber);
    }
}

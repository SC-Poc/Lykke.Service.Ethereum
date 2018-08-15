using System.Threading.Tasks;
using Lykke.Service.EthereumWorker.Core.DistributedLock;
using Lykke.Service.EthereumWorker.Core.Domain;


namespace Lykke.Service.EthereumWorker.Core.Repositories
{
    public interface IBlockchainIndexationStateRepository
    {
        Task<BlockchainIndexationState> GetOrCreateAsync();

        Task UpdateAsync(
            BlockchainIndexationState state);

        Task<IDistributedLockToken> WaitLockAsync();
    }
}

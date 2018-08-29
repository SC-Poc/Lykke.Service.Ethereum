using System.Threading.Tasks;

namespace Lykke.Service.EthereumWorker.Core.DistributedLock
{
    public interface IDistributedLock
    {
        Task<IDistributedLockToken> WaitAsync();
    }
}

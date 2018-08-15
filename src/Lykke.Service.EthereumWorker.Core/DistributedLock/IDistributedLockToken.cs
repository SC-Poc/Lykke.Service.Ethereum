using System.Threading.Tasks;

namespace Lykke.Service.EthereumWorker.Core.DistributedLock
{
    public interface IDistributedLockToken
    {
        Task ReleaseAsync();
    }
}

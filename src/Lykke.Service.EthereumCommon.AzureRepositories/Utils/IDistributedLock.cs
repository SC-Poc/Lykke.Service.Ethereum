using System;
using System.Threading.Tasks;

namespace Lykke.Service.EthereumCommon.AzureRepositories.Utils
{
    public interface IDistributedLock
    {
        Task<IDisposable> WaitAsync();
    }
}

using System;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.EthereumWorker.Core.DistributedLock;

namespace Lykke.Service.EthereumWorker.AzureRepositories.AzureBlockLock
{
    internal class ReloadingConnectionStringOnFailureAzureLockDecorator : ReloadingOnFailureDecoratorBase<IDistributedLock>, IDistributedLock
    {
        public ReloadingConnectionStringOnFailureAzureLockDecorator(
            Func<bool, Task<IDistributedLock>> makeStorage)
        {
            MakeStorage = makeStorage;
        }
        
        
        protected override Func<bool, Task<IDistributedLock>> MakeStorage { get; }


        public Task<IDistributedLockToken> WaitAsync()
        {
            return WrapAsync(x => x.WaitAsync());
        }
    }
}

using System;
using System.Threading.Tasks;
using AzureStorage;

namespace Lykke.Service.EthereumCommon.AzureRepositories.Utils
{
    internal class ReloadingConnectionStringOnFailureAzureLockDecorator : ReloadingOnFailureDecoratorBase<IDistributedLock>, IDistributedLock
    {
        public ReloadingConnectionStringOnFailureAzureLockDecorator(
            Func<bool, Task<IDistributedLock>> makeStorage)
        {
            MakeStorage = makeStorage;
        }
        
        
        protected override Func<bool, Task<IDistributedLock>> MakeStorage { get; }


        public Task<IDisposable> WaitAsync()
        {
            return WrapAsync(x => x.WaitAsync());
        }
    }
}

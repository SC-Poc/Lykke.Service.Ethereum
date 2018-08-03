using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.EthereumWorker.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public WorkerSettings WorkerService { get; set; }        
    }
}

using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.EthereumWorker.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public EthereumWorkerSettings EthereumWorkerService { get; set; }        
    }
}

using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.EthereumApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public ApiSettings ApiService { get; set; }        
    }
}

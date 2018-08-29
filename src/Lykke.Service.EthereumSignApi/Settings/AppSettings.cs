using JetBrains.Annotations;
using Lykke.Sdk.Settings;

namespace Lykke.Service.EthereumSignApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AppSettings : BaseAppSettings
    {
        public SignApiSettings SignApiService { get; set; }        
    }
}

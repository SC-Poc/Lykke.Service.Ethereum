using JetBrains.Annotations;

namespace Lykke.Service.EthereumSignApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class SignApiSettings
    {
        public DbSettings Db { get; set; }
    }
}

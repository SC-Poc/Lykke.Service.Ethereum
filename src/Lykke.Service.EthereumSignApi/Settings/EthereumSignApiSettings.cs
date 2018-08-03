using JetBrains.Annotations;

namespace Lykke.Service.EthereumSignApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EthereumSignApiSettings
    {
        public DbSettings Db { get; set; }
    }
}

using JetBrains.Annotations;

namespace Lykke.Service.EthereumApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EthereumApiSettings
    {
        public DbSettings Db { get; set; }
    }
}

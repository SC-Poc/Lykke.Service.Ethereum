using JetBrains.Annotations;

namespace Lykke.Service.EthereumApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ApiSettings
    {
        public DbSettings Db { get; set; }
    }
}

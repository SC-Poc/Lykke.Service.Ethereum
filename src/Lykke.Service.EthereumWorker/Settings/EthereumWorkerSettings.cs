using JetBrains.Annotations;

namespace Lykke.Service.EthereumWorker.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class EthereumWorkerSettings
    {
        public DbSettings Db { get; set; }
    }
}

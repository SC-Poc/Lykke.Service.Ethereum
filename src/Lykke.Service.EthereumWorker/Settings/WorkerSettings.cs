using JetBrains.Annotations;

namespace Lykke.Service.EthereumWorker.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class WorkerSettings
    {
        public DbSettings Db { get; set; }
    }
}

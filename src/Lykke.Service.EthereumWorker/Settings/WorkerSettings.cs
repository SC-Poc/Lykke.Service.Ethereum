using System;
using JetBrains.Annotations;

namespace Lykke.Service.EthereumWorker.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class WorkerSettings
    {
        public int BalanceObservationMaxDegreeOfParallelism { get; set; }
        
        public int BlockchainIndexingMaxDegreeOfParallelism { get; set; }
        
        public int ConfirmationLevel { get; set; } 
        
        public TimeSpan BlockLockDuration { get; set; }
        
        public DbSettings Db { get; set; }
        
        public string ParityNodeUrl { get; set; }
        
        public int TransactionMonitoringMaxDegreeOfParallelism { get; set; }
    }
}

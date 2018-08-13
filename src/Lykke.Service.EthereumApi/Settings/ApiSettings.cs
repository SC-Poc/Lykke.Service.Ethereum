using JetBrains.Annotations;

namespace Lykke.Service.EthereumApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ApiSettings
    {
        public DbSettings Db { get; set; }
        
        public string MaximalGasPrice { get; set; }
        
        public string MinimalGasPrice { get; set; }
        
        public string MinimalTransactionAmount { get; set; }
        
        public string ParityNodeUrl { get; set; }
    }
}

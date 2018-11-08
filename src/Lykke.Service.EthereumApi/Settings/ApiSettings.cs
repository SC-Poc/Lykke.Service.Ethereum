using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.SettingsReader.Attributes;


namespace Lykke.Service.EthereumApi.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class ApiSettings
    {
        [Optional]
        public ChaosSettings Chaos { get; set; }
        
        public DbSettings Db { get; set; }
        
        public int GasAmountReservePercentage { get; set; }
        
        public string MaximalGasAmount { get; set; }
        
        public string MaximalGasPrice { get; set; }
        
        public string MinimalGasPrice { get; set; }
        
        public string MinimalTransactionAmount { get; set; }
        
        public string ParityNodeUrl { get; set; }
    }
}

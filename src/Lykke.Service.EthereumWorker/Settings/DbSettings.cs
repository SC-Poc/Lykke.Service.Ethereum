using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.EthereumWorker.Settings
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class DbSettings
    {
        [AzureTableCheck]
        public string DataConnString { get; set; }
        
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}

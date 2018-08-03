using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.EthereumWorker.Settings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}

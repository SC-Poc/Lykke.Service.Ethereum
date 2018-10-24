using Lykke.AzureStorage.Tables;

namespace Lykke.Service.EthereumCommon.AzureRepositories.Entities
{
    public class BlacklistedAddressEntity : AzureTableEntity
    {
        public string Reason { get; set; }
    }
}

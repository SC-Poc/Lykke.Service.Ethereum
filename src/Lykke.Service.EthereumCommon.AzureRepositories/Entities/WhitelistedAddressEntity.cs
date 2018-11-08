using System.Numerics;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.EthereumCommon.AzureRepositories.Entities
{
    public class WhitelistedAddressEntity : AzureTableEntity
    {
        public BigInteger MaxGasAmount { get; set; }
        
    }
}

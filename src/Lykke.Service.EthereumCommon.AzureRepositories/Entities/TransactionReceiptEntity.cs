using System.Numerics;
using Lykke.AzureStorage.Tables;


namespace Lykke.Service.EthereumCommon.AzureRepositories.Entities
{
    public class TransactionReceiptEntity : AzureTableEntity
    {
        public BigInteger Amount { get; set; }
        
        public BigInteger BlockNumber { get; set; }
        
        public string From { get; set; }

        public string Hash { get; set; }
        
        public BigInteger TransactionTimestamp { get; set; }

        public string To { get; set; }
    }
}

using System;
using System.Numerics;
using Lykke.AzureStorage.Tables;
using Lykke.Service.EthereumCommon.Core;

namespace Lykke.Service.EthereumCommon.AzureRepositories.Entities
{
    public class TransactionEntity : AzureTableEntity
    {
        public BigInteger Amount { get; set; }

        public BigInteger? BlockNumber { get; set; }
        
        public DateTime? BroadcastedOn { get; set; }
        
        public DateTime BuiltOn { get; set; }
        
        public DateTime? CompletedOn { get; set; }

        public string Data { get; set; }
        
        public DateTime? DeletedOn { get; set; }
        
        public string Error { get; set; }
        
        public string From { get; set; }

        public string Hash { get; set; }
        
        public Guid OperationId { get; set; }

        public string SignedData { get; set; }

        public TransactionState State { get; set; }

        public string To { get; set; }
    }
}

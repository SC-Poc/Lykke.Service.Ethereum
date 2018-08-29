using System;
using System.Numerics;
using Lykke.AzureStorage.Tables;

namespace Lykke.Service.EthereumWorker.AzureRepositories.Entities
{
    public class BlockIndexationLockEntity : AzureTableEntity
    {
        public BigInteger BlockNumber { get; set; }
        
        public DateTime LockedOn { get; set; }
    }
}

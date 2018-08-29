using System;
using System.Numerics;

namespace Lykke.Service.EthereumWorker.Core.Domain
{
    public class BlockIndexationLock
    {
        public BigInteger BlockNumber { get; set; }
        
        public DateTime LockedOn { get; set; }
    }
}

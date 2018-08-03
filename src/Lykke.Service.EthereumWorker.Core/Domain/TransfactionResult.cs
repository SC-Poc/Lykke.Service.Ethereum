using System.Numerics;

namespace Lykke.Service.EthereumWorker.Core.Domain
{
    public class TransfactionResult
    {
        public BigInteger BlockNumber { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public bool IsFailed { get; set; }
    }
}

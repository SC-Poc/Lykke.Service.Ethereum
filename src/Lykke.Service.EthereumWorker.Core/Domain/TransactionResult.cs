using System.Numerics;

namespace Lykke.Service.EthereumWorker.Core.Domain
{
    public class TransactionResult
    {
        public BigInteger BlockNumber { get; set; }
        
        public string Error { get; set; }
        
        public bool IsCompleted { get; set; }
        
        public bool IsFailed { get; set; }
    }
}

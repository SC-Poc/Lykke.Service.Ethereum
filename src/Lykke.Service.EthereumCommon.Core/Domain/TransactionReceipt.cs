using System.Numerics;

namespace Lykke.Service.EthereumCommon.Core.Domain
{
    public class TransactionReceipt
    {
        public BigInteger Amount { get; set; }
        
        public BigInteger BlockNumber { get; set; }
        
        public string From { get; set; }
        
        public string Hash { get; set; }
        
        public BigInteger Timestamp { get; set; }
        
        public string To { get; set; }
    }
}

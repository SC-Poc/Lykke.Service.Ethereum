using System.Numerics;
using MessagePack;

namespace Lykke.Service.EthereumCommon.Core.Domain
{
    [MessagePackObject]
    public class UnsignedTransaction
    {
        [Key(0)]
        public BigInteger Amount { get; set; }
        
        [Key(1)]
        public BigInteger GasAmount { get; set; }
        
        [Key(2)]
        public BigInteger GasPrice { get; set; }
        
        [Key(3)]
        public BigInteger Nonce { get; set; }
        
        [Key(4)]
        public string To { get; set; }
    }
}

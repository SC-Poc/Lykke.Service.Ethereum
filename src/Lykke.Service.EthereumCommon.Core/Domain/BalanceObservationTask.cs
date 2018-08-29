using System.Numerics;
using MessagePack;

namespace Lykke.Service.EthereumCommon.Core.Domain
{
    [MessagePackObject]
    public class BalanceObservationTask
    {
        [Key(0)]
        public string Address { get; set; }
        
        [Key(1)]
        public BigInteger BlockNumber { get; set; }
    }
}

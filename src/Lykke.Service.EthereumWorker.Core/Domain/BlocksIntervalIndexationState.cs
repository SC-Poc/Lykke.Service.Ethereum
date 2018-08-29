using System.Numerics;
using System.Runtime.Serialization;

namespace Lykke.Service.EthereumWorker.Core.Domain
{
    [DataContract]
    public sealed class BlocksIntervalIndexationState
    {
        public BlocksIntervalIndexationState(
            BigInteger from,
            BigInteger to,
            bool isIndexed)
        {
            From = from;
            IsIndexed = isIndexed;
            To = to;
        }

        [DataMember(Order = 0)]
        public BigInteger From { get; }
        
        [DataMember(Order = 2)]
        public bool IsIndexed { get; }
        
        [DataMember(Order = 1)]
        public BigInteger To { get; }
    }
}

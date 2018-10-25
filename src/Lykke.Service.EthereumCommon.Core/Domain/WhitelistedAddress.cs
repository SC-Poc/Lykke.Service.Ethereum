using System.Numerics;

namespace Lykke.Service.EthereumCommon.Core.Domain
{
    public class WhitelistedAddress
    {
        public string Address { get; set; }
        
        public BigInteger MaxGasAmount { get; set; }
    }
}

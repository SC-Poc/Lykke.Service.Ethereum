namespace Lykke.Service.EthereumCommon.Core.Domain
{
    public class BlacklistedAddress
    {
        public BlacklistedAddress(
            string address,
            string reason)
        {
            Address = address;
            BlacklistingReason = reason;
        }
        
        public string Address { get; }
        
        public string BlacklistingReason { get; }
    }
}

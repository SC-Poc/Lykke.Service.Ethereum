namespace Lykke.Service.EthereumApi.Core.Domain
{
    public class BlacklistedAddress
    {
        public BlacklistedAddress(
            string address,
            string blacklistingReason)
        {
            Address = address;
            BlacklistingReason = blacklistingReason;
        }

        public string Address { get; }
        
        public string BlacklistingReason { get; }
    }
}

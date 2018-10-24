namespace Lykke.Service.EthereumCommon.Core.Domain
{
    public class BlacklistedAddress
    {
        public BlacklistedAddress(
            string address,
            string reason)
        {
            Address = address;
            Reason = reason;
        }
        
        public string Address { get; }
        
        public string Reason { get; }
    }
}

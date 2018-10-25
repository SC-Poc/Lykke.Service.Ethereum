namespace Lykke.Service.EthereumApi.Models
{
    public class BlacklistAddressRequest : AddressRequest
    {
        public string BlacklistingReason { get; set; }
    }
}

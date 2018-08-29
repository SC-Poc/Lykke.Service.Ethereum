namespace Lykke.Service.EthereumSignApi.Core.Services.Interfaces
{
    public interface IWalletService
    {
        (string Address, string PrivateKey) CreateWallet();
    }
}

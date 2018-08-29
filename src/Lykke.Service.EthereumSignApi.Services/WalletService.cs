using JetBrains.Annotations;
using Lykke.Service.EthereumSignApi.Core.Services.Interfaces;
using Nethereum.Signer;


namespace Lykke.Service.EthereumSignApi.Services
{
    [UsedImplicitly]
    public class WalletService : IWalletService
    {
        public (string Address, string PrivateKey) CreateWallet()
        {
            var ethECKey = EthECKey.GenerateKey();

            return 
            (
                ethECKey.GetPublicAddress(),
                ethECKey.GetPrivateKey()
            );
        }
    }
}

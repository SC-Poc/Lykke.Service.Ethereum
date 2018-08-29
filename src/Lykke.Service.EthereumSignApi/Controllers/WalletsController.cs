using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Wallets;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumSignApi.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace Lykke.Service.EthereumSignApi.Controllers
{
    [PublicAPI, Route("api/wallets")]
    public class WalletsController : Controller
    {
        private readonly IWalletService _walletService;

        public WalletsController(
            IWalletService walletService)
        {
            _walletService = walletService;
        }
        

        [HttpPost]
        public ActionResult<WalletResponse> CreateWallet()
        {
            var wallet = _walletService.CreateWallet();
            
            return new WalletResponse
            {
                PrivateKey = wallet.PrivateKey,
                PublicAddress = Address.AddChecksum(wallet.Address)
            };
        }
    }
}

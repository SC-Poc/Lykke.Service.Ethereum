using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Balances;
using Lykke.Service.EthereumApi.Models;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumApi.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.EthereumApi.Controllers
{
    [PublicAPI, Route("api/balances")]
    public class BalancesController : Controller
    {
        private readonly IBalanceService _balanceService;


        public BalancesController(
            IBalanceService balanceService)
        {
            _balanceService = balanceService;
        }

        
        [HttpPost("{address}/observation")]
        public async Task<ActionResult> AddAddressToObservationList(
            AddressRequest request)
        {
            var address = request.Address.ToLowerInvariant();
            
            if (await _balanceService.BeginObservationIfNotObservingAsync(address))
            {
                return Ok();
            }
            else
            {
                return Conflict();
            }
        }
        
        [HttpDelete("{address}/observation")]
        public async Task<ActionResult> DeleteAddressFromObservationList(
            AddressRequest request)
        {
            var address = request.Address.ToLowerInvariant();
            
            if (await _balanceService.EndObservationIfObservingAsync(address))
            {
                return Ok();
            }
            else
            {
                return NoContent();
            }
        }

        [HttpGet]
        public async Task<ActionResult<PaginationResponse<WalletBalanceContract>>> GetBalanceList(
            PaginationRequest request)
        {
            var (balances, continuation) = await _balanceService.GetTransferableBalancesAsync
            (
                request.Take,
                request.Continuation
            );

            return new PaginationResponse<WalletBalanceContract>
            {
                Continuation = continuation,
                Items = balances.Select(x => new WalletBalanceContract
                {
                    Address = x.Address,
                    AssetId = Constants.AssetId,
                    Balance = x.Amount.ToString(),
                    Block = (long) x.BlockNumber
                }).ToList()
            };
        }
    }
}

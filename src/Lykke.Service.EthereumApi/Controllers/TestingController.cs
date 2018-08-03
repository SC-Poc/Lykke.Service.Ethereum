using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Testing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;


namespace Lykke.Service.EthereumApi.Controllers
{
    [PublicAPI, Route("/api/testing")]
    public class TestingController : Controller
    {
        #region Not Implemented Endpoints
        
        [HttpPost("transfers")]
        public ActionResult Transfer(
            [FromBody] TestingTransferRequest request)
                => StatusCode(StatusCodes.Status501NotImplemented);
        
        [HttpDelete("storage")]
        public ActionResult ClearStorage()
            => StatusCode(StatusCodes.Status501NotImplemented);
        
        #endregion
    }
}

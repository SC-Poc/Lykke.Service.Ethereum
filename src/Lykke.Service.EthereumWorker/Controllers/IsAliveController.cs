using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.EthereumCommon;
using Lykke.Service.EthereumCommon.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

namespace Lykke.Service.EthereumWorker.Controllers
{
    [Route("api/isalive")]
    public class IsAliveController : Controller
    {
        [HttpGet]        
        public ActionResult<IsAliveResponse> Get()
        {
            return new IsAliveResponse
            {
                Name = PlatformServices.Default.Application.ApplicationName,
                Version = PlatformServices.Default.Application.ApplicationVersion,
                Env = ProgramBase.EnvInfo,
                IsDebug = Constants.IsDebug,
            };
        }
    }
}

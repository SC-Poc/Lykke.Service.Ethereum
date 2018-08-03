using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.EthereumApi.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AssetRequest
    {
        [FromRoute]
        public string AssetId { get; set; }
    }
}

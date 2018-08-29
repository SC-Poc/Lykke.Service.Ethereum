using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.EthereumApi.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class AddressRequest
    {
        [FromRoute]
        public string Address { get; set; }
    }
}

using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.EthereumApi.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TransactionRequest
    {
        [FromRoute]
        public Guid TransactionId { get; set; }
    }
}

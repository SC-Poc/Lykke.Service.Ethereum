using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.EthereumApi.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class TransactionHistoryRequest
    {
        public TransactionHistoryRequest()
        {
            AfterHash = string.Empty;
        }
        
        [FromRoute]
        public string Address { get; set; }
        
        [FromQuery]
        public int Take { get; set; }
        
        [FromQuery]
        public string AfterHash { get; set; }
    }
}

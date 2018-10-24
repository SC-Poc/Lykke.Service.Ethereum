using JetBrains.Annotations;

namespace Lykke.Service.EthereumApi.Models
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class BlacklistingReasonResponse
    {
        public string Reason { get; set; }
    }
}

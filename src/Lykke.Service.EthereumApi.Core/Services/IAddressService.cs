using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface IAddressService
    {
        Task<bool> ValidateAsync(
            [NotNull] string address);
    }
}

using System.Numerics;
using System.Threading.Tasks;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface IBlockchainService
    {
        Task<BigInteger> GetBalanceAsync(
            string address);

        Task<string> BroadcastTransactionAsync(
            string signedTxData);

        Task<string> BuildTransactionAsync(
            string to,
            BigInteger amount);
    }
}

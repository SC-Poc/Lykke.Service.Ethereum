using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface IBlockchainService
    {
        Task<BigInteger> GetBalanceAsync(
            [NotNull] string address);

        Task<string> BroadcastTransactionAsync(
            [NotNull] string signedTxData);

        Task<string> BuildTransactionAsync(
            [NotNull] string from,
            [NotNull] string to,
            BigInteger amount,
            BigInteger gasPrice);

        Task<BigInteger> EstimateGasPriceAsync(
            string to,
            BigInteger amount);
        
        Task<bool> IsWalletAsync(
            [NotNull] string address);
    }
}

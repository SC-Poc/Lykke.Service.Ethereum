using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface IBlockchainService
    {
        [ItemNotNull]
        Task<string> BroadcastTransactionAsync(
            [NotNull] string signedTxData);

        [ItemNotNull]
        Task<string> BuildTransactionAsync(
            [NotNull] string from,
            [NotNull] string to,
            BigInteger amount,
            BigInteger gasAmount,
            BigInteger gasPrice);

        Task<BigInteger?> TryEstimateGasAmountAsync(
            [NotNull] string from,
            [NotNull] string to,
            BigInteger amount);
        
        Task<BigInteger> EstimateGasPriceAsync();

        Task<BigInteger> GetBalanceAsync(
            [NotNull] string address);
        
        Task<bool> IsWalletAsync(
            [NotNull] string address);
    }
}

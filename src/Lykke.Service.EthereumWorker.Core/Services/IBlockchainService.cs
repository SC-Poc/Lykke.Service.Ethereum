using System.Numerics;
using System.Threading.Tasks;
using Lykke.Service.EthereumWorker.Core.Domain;


namespace Lykke.Service.EthereumWorker.Core.Services
{
    public interface IBlockchainService
    {   
        Task<BigInteger> GetBalanceAsync(
            string address,
            BigInteger blockNumber);
        
        Task<BigInteger> GetBestTrustedBlockNumberAsync();

        Task<TransfactionResult> GetTransactionResultAsync(
            string hash);
    }
}

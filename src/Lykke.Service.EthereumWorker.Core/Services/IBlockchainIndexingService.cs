using System.Numerics;
using System.Threading.Tasks;


namespace Lykke.Service.EthereumWorker.Core.Services
{
    public interface IBlockchainIndexingService
    {
        Task<BigInteger[]> GetNonIndexedBlocksAsync(
            int take);

        Task<BigInteger[]> IndexBlocksAsync(
            BigInteger[] blockNumbers);

        Task MarkBlocksAsIndexed(
            BigInteger[] blockNumbers);
    }
}

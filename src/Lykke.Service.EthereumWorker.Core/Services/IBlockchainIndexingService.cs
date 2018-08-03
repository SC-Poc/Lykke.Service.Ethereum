using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;

namespace Lykke.Service.EthereumWorker.Core.Services
{
    public interface IBlockchainIndexingService
    {
        Task<IEnumerable<BigInteger>> GetNonIndexedBlocksAsync(
            int take);

        Task<IEnumerable<BigInteger>> IndexBlocksAsync(
            IEnumerable<BigInteger> blockNumbers);

        Task MarkBlocksAsIndexed(
            IEnumerable<BigInteger> blockNumbers);
    }
}

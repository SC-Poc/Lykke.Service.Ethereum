using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumWorker.Core.QueueConsumer;
using Lykke.Service.EthereumWorker.Core.Services;

namespace Lykke.Service.EthereumWorker.QueueConsumers
{
    [UsedImplicitly]
    public class BlockchainIndexationQueueConsumer : QueueConsumer<IEnumerable<BigInteger>>
    {
        private readonly IBlockchainIndexingService _blockchainIndexingService;
        
        
        public BlockchainIndexationQueueConsumer(
            IBlockchainIndexingService blockchainIndexingService,
            Settings settings)
            : base(maxDegreeOfParallelism: settings.MaxDegreeOfParallelism, emptyQueueCheckInterval: 5000)
        {
            _blockchainIndexingService = blockchainIndexingService;
        }

        
        protected override async Task<(bool, IEnumerable<BigInteger>)> TryGetNextTaskAsync()
        {
            var nonIndexedBlockBatch = (await _blockchainIndexingService.GetNonIndexedBlocksAsync(take: 10))
                .ToList();

            return (nonIndexedBlockBatch.Any(), nonIndexedBlockBatch);
        }

        protected override async Task ProcessTaskAsync(
            IEnumerable<BigInteger> nonIndexedBlockBatch)
        {
            var indexeBlocks = await _blockchainIndexingService.IndexBlocksAsync(nonIndexedBlockBatch);

            await _blockchainIndexingService.MarkBlocksAsIndexed(indexeBlocks);
        }
        
        
        public class Settings
        {
            public int MaxDegreeOfParallelism { get; set; }
        }
    }
}

using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumWorker.Core.QueueConsumer;
using Lykke.Service.EthereumWorker.Core.Services;


namespace Lykke.Service.EthereumWorker.QueueConsumers
{
    [UsedImplicitly]
    public class BlockchainIndexationQueueConsumer : QueueConsumer<BigInteger[]>
    {
        private readonly IBlockchainIndexingService _blockchainIndexingService;
        private readonly ILog _log;
        private readonly int _maxDegreeOfParallelism;
        
        
        public BlockchainIndexationQueueConsumer(
            IBlockchainIndexingService blockchainIndexingService,
            ILogFactory logFactory,
            Settings settings)
            : base(emptyQueueCheckInterval: 5000)
        {
            _blockchainIndexingService = blockchainIndexingService;
            _log = logFactory.CreateLog(this);
            _maxDegreeOfParallelism = settings.MaxDegreeOfParallelism;
        }

        
        protected override async Task<(bool, BigInteger[])> TryGetNextTaskAsync()
        {
            var nonIndexedBlockBatch = await _blockchainIndexingService.GetNonIndexedBlocksAsync(take: _maxDegreeOfParallelism);

            return (nonIndexedBlockBatch.Any(), nonIndexedBlockBatch);
        }

        protected override async Task ProcessTaskAsync(
            BigInteger[] nonIndexedBlockBatch)
        {
            var indexeBlocks = await _blockchainIndexingService.IndexBlocksAsync(nonIndexedBlockBatch);

            await _blockchainIndexingService.MarkBlocksAsIndexed(indexeBlocks);
        }
        
        public override void Start()
        {
            _log.Info("Starting blockchain observation...");
            
            base.Start();
            
            _log.Info("Blockchain observation started.");
        }

        public override void Stop()
        {
            _log.Info("Stopping blockchain observation...");
            
            base.Stop();
            
            _log.Info("Blockchain observation stopped.");
        }
        
        public class Settings
        {
            public int MaxDegreeOfParallelism { get; set; }
        }
    }
}

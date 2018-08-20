using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Blob;
using Lykke.Service.EthereumCommon.AzureRepositories;
using Lykke.Service.EthereumWorker.AzureRepositories.AzureBlockLock;
using Lykke.Service.EthereumWorker.Core.DistributedLock;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.Service.EthereumWorker.Core.Repositories;
using Lykke.SettingsReader;
using MessagePack;


namespace Lykke.Service.EthereumWorker.AzureRepositories
{
    public class BlockchainIndexationStateRepository : RepositoryBase, IBlockchainIndexationStateRepository
    {
        private const string Container = "blockchain-indexation-state";
        private const string DataKey = ".bin";
        private const string LockKey = ".lock";


        private readonly IBlobStorage _blobStorage;
        private readonly IDistributedLock _lock;


        private BlockchainIndexationStateRepository(
            IBlobStorage blobStorage,
            IDistributedLock @lock)
        {
            _blobStorage = blobStorage;
            _lock = @lock;
        }

        public static BlockchainIndexationStateRepository Create(
            IReloadingManager<string> connectionString)
        {
            return new BlockchainIndexationStateRepository
            (
                AzureBlobStorage.Create(connectionString),
                AzureBlobLock.Create
                (
                    connectionStringManager: connectionString,
                    container: Container,
                    key: LockKey,
                    lockDuration: 60
                )
            );
        }
        
        public async Task<BlockchainIndexationState> GetOrCreateAsync()
        {
            if (await _blobStorage.HasBlobAsync(Container, DataKey))
            {
                using (var stream = await _blobStorage.GetAsync(Container, DataKey))
                {
                    return BlockchainIndexationState.Restore
                    (
                        await MessagePackSerializer.DeserializeAsync<IEnumerable<BlocksIntervalIndexationState>>(stream)
                    );
                }
            }
            else
            {
                return BlockchainIndexationState.Create();
            }
        }

        public async Task UpdateAsync(
            BlockchainIndexationState state)
        {
            using (var stream = new MemoryStream())
            {
                await MessagePackSerializer.SerializeAsync(stream, (IEnumerable<BlocksIntervalIndexationState>) state);
                
                await _blobStorage.SaveBlobAsync(Container, DataKey, stream);
            }
        }

        public Task<IDistributedLockToken> WaitLockAsync()
        {
            return _lock.WaitAsync();
        }
    }
}

using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.AzureRepositories;
using Lykke.Service.EthereumWorker.AzureRepositories.Entities;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.Service.EthereumWorker.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.EthereumWorker.AzureRepositories
{
    public class BlockIndexationLockRepository : RepositoryBase, IBlockIndexationLockRepository
    {
        private readonly INoSQLTableStorage<BlockIndexationLockEntity> _locks;
        
        
        private BlockIndexationLockRepository(
            INoSQLTableStorage<BlockIndexationLockEntity> locks)
        {
            _locks = locks;
        }
        
        
        public static IBlockIndexationLockRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            var locks = AzureTableStorage<BlockIndexationLockEntity>.Create
            (
                connectionString,
                "BlockIndexationLocks",
                logFactory
            );
            
            return new BlockIndexationLockRepository
            (
                locks: locks
            );
        }
        
        
        public async Task DeleteIfExistsAsync(
            BigInteger blockNumber)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<BlockIndexationLock>> GetAsync()
        {
            throw new System.NotImplementedException();
        }

        public async Task InsertOrReplaceAsync(
            BigInteger blockNumber)
        {
            throw new System.NotImplementedException();
        }
        
        #region Key Builders
        
        private static (string PartitionKey, string RowKey) GetReceiptKeys(
            BigInteger blockNumber)
        {
            var blockNumbberString = blockNumber.ToString();
            
            return (blockNumbberString.CalculateHexHash32(3), blockNumbberString);
        }
        
        #endregion
    }
}

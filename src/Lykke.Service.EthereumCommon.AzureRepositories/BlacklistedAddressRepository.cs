using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.AzureRepositories.Entities;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.EthereumCommon.AzureRepositories
{
    public class BlacklistedAddressRepository : IBlacklistedAddressRepository
    {
        private readonly INoSQLTableStorage<BlacklistedAddressEntity> _blacklistedAddresses;

        private BlacklistedAddressRepository(
            INoSQLTableStorage<BlacklistedAddressEntity> blacklistedAddresses)
        {
            _blacklistedAddresses = blacklistedAddresses;
        }

        public static IBlacklistedAddressRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            var blacklistedAddresses = AzureTableStorage<BlacklistedAddressEntity>.Create
            (
                connectionString,
                "BlacklistedAddresses",
                logFactory
            );
            
            return new BlacklistedAddressRepository(blacklistedAddresses);
        }
        
        
        public Task<bool> AddIfNotExistsAsync(
            string address,
            string reason)
        {
            var (partitionKey, rowKey) = GetKeys(address);
            
            return _blacklistedAddresses.TryInsertAsync
            (
                new BlacklistedAddressEntity
                {
                    Reason = reason,
                    
                    PartitionKey = partitionKey,
                    RowKey = rowKey
                }
            );
        }

        public async Task<bool> ContainsAsync(
            string address)
        {
            var blacklistedAddress = await TryGetAsync(address);
            
            return blacklistedAddress != null;
        }
        
        public async Task<(IEnumerable<BlacklistedAddress> Addresses, string ContinuationToken)> GetAllAsync(
            int take,
            string continuationToken)
        {
            IEnumerable<BlacklistedAddressEntity> addresses;
            
            (addresses, continuationToken) = await _blacklistedAddresses.GetDataWithContinuationTokenAsync(take, continuationToken);

            return (addresses.Select(x => new BlacklistedAddress(x.RowKey, x.Reason)), continuationToken);
        }

        public async Task<BlacklistedAddress> TryGetAsync(
            string address)
        {
            var (partitionKey, rowKey) = GetKeys(address);

            var entity = await _blacklistedAddresses.GetDataAsync
            (
                partition: partitionKey,
                row: rowKey
            );

            if (entity != null)
            {
                return new BlacklistedAddress
                (
                    address: entity.RowKey,
                    reason: entity.Reason
                );
            }
            else
            {
                return null;
            }
        }
        
        public Task<bool> RemoveIfExistsAsync(
            string address)
        {
            var (partitionKey, rowKey) = GetKeys(address);

            return _blacklistedAddresses.DeleteIfExistAsync
            (
                partitionKey: partitionKey,
                rowKey: rowKey
            );
        }
        
        
        #region Key Builders
        
        private static (string, string) GetKeys(
            string address)
        {
            var partitionKey = address.CalculateHexHash32(3);
            var rowKey = address;

            return (partitionKey, rowKey);
        }
        
        #endregion
    }
}

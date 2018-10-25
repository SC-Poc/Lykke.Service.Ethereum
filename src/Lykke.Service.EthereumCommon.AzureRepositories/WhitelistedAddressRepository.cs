using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.AzureRepositories.Entities;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Service.EthereumCommon.AzureRepositories
{
    public class WhitelistedAddressRepository : IWhitelistedAddressRepository
    {
        private readonly INoSQLTableStorage<WhitelistedAddressEntity> _whitelistedAddresses;

        private WhitelistedAddressRepository(
            INoSQLTableStorage<WhitelistedAddressEntity> whitelistedAddresses)
        {
            _whitelistedAddresses = whitelistedAddresses;
        }


        public static IWhitelistedAddressRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            var whitelistedAddresses = AzureTableStorage<WhitelistedAddressEntity>.Create
            (
                connectionString,
                "WhitelistedAddresses",
                logFactory
            );
            
            return new WhitelistedAddressRepository(whitelistedAddresses);
        }
        
        public Task<bool> AddIfNotExistsAsync(
            string address)
        {
            var (partitionKey, rowKey) = GetKeys(address);
            
            return _whitelistedAddresses.TryInsertAsync
            (
                new WhitelistedAddressEntity
                {
                    PartitionKey = partitionKey,
                    RowKey = rowKey
                }
            );
        }

        public async Task<bool> ContainsAsync(string address)
        {
            var (partitionKey, rowKey) = GetKeys(address);

            var entity = await _whitelistedAddresses.GetDataAsync
            (
                partition: partitionKey,
                row: rowKey
            );

            return entity != null;
        }

        public async Task<(IEnumerable<string> Addresses, string ContinuationToken)> GetAllAsync(
            int take,
            string continuationToken)
        {
            IEnumerable<WhitelistedAddressEntity> addresses;
            
            (addresses, continuationToken) = await _whitelistedAddresses.GetDataWithContinuationTokenAsync(take, continuationToken);

            return (addresses.Select(x => x.RowKey), continuationToken);
        }

        public Task<bool> RemoveIfExistsAsync(string address)
        {
            var (partitionKey, rowKey) = GetKeys(address);

            return _whitelistedAddresses.DeleteIfExistAsync
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

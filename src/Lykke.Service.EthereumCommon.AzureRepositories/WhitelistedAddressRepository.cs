using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            string address,
            BigInteger maxGasAmount)
        {
            var (partitionKey, rowKey) = GetKeys(address);
            
            return _whitelistedAddresses.TryInsertAsync
            (
                new WhitelistedAddressEntity
                {
                    MaxGasAmount = maxGasAmount,
                    
                    PartitionKey = partitionKey,
                    RowKey = rowKey
                }
            );
        }

        public async Task<bool> ContainsAsync(
            string address)
        {
            return (await TryGetAsync(address)) != null;
        }

        public async Task<(IEnumerable<WhitelistedAddress> Addresses, string ContinuationToken)> GetAllAsync(
            int take,
            string continuationToken)
        {
            IEnumerable<WhitelistedAddressEntity> addresses;
            
            (addresses, continuationToken) = await _whitelistedAddresses.GetDataWithContinuationTokenAsync(take, continuationToken);

            return (addresses.Select(x => new WhitelistedAddress(x.RowKey, x.MaxGasAmount)), continuationToken);
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

        public async Task<WhitelistedAddress> TryGetAsync(
            string address)
        {
            var (partitionKey, rowKey) = GetKeys(address);

            var entity = await _whitelistedAddresses.GetDataAsync
            (
                partition: partitionKey,
                row: rowKey
            );

            if (entity != null)
            {
                return new WhitelistedAddress
                (
                    address: entity.PartitionKey,
                    maxGasAmount: entity.MaxGasAmount
                );
            }
            else
            {
                return null;
            }
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

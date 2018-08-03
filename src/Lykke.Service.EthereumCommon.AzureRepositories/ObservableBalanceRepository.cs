using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.AzureRepositories.Entities;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;


namespace Lykke.Service.EthereumCommon.AzureRepositories
{
    [UsedImplicitly]
    public class ObservableBalanceRepository : RepositoryBase, IObservableBalanceRepository
    {
        private readonly INoSQLTableStorage<ObservableBalanceEntity> _observableAccountStates;


        private ObservableBalanceRepository(
            INoSQLTableStorage<ObservableBalanceEntity> observableAccountStates)
        {
            _observableAccountStates = observableAccountStates;
        }

        public static IObservableBalanceRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            var observableAccountStates = AzureTableStorage<ObservableBalanceEntity>.Create
            (
                connectionString,
                "ObservableBalances",
                logFactory
            );
            
            return new ObservableBalanceRepository(observableAccountStates);
        }
        
        
        public Task<bool> CreateIfNotExistsAsync(
            string address)
        {
            var (partitionKey, rowKey) = GetKeys(address);

            var entity = new ObservableBalanceEntity
            {
                PartitionKey = partitionKey,
                RowKey = rowKey
            };
            
            return _observableAccountStates.CreateIfNotExistsAsync(entity);
        }
        
        public Task<bool> DeleteIfExistsAsync(
            string address)
        {
            var (partitionKey, rowKey) = GetKeys(address);
            
            return _observableAccountStates.DeleteIfExistAsync
            (
                partitionKey: partitionKey,
                rowKey: rowKey
            );
        }

        public async Task<bool> ExistsAsync(
            string address)
        {
            var (partitionKey, rowKey) = GetKeys(address);
            
            var entity = await _observableAccountStates.GetDataAsync
            (
                partition: partitionKey,
                row: rowKey
            );

            return entity != null;
        }
        
        public async Task<(IEnumerable<Balance> Balances, string ContinuationToken)> GetAllTransferableBalancesAsync(
            int take,
            string continuationToken)
        {
            var filterCondition = TableQuery
                .GenerateFilterCondition(
                    nameof(ObservableBalanceEntity.Amount), QueryComparisons.NotEqual, "0");
            
            var rangeQuery = new TableQuery<ObservableBalanceEntity>()
                .Where(filterCondition);

            var (entities, newContinuationToken) = await _observableAccountStates
                .GetDataWithContinuationTokenAsync(rangeQuery, take, continuationToken);

            var balances = entities.Select(x => new Balance
            {
                Address = x.RowKey,
                Amount = x.Amount,
                BlockNumber = x.BlockNumber
            });

            return (balances, newContinuationToken);
        }

        public async Task<Balance> TryGetAsync(
            string address)
        {
            var (partitionKey, rowKey) = GetKeys(address);

            var entity = await _observableAccountStates.GetDataAsync(partition: partitionKey, row: rowKey);

            if (entity != null)
            {
                return new Balance
                {
                    Address = address,
                    Amount = entity.Amount,
                    BlockNumber = entity.BlockNumber
                };
            }
            else
            {
                return null;
            }
        }

        public async Task UpdateSafelyAsync(
            Balance balance)
        {
            ObservableBalanceEntity Merge(ObservableBalanceEntity entity)
            {
                if (balance.BlockNumber > entity.BlockNumber)
                {
                    entity.Amount = balance.Amount;
                    entity.BlockNumber = balance.BlockNumber;
                }
                
                return entity;
            }

            var (partitionKey, rowKey) = GetKeys(balance.Address);
            
            await _observableAccountStates.MergeAsync
            (
                partitionKey: partitionKey,
                rowKey: rowKey,
                mergeAction: Merge
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

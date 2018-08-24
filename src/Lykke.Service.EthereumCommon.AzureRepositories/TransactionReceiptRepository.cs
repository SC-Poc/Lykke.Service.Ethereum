using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.AzureRepositories.Entities;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.SettingsReader;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Lykke.Service.EthereumCommon.AzureRepositories
{
    [UsedImplicitly]
    public class TransactionReceiptRepository : RepositoryBase, ITransactionReceiptRepository
    {
        private readonly INoSQLTableStorage<TransactionReceiptEntity> _transactionReceipts;
        
        private readonly INoSQLTableStorage<AzureIndex> _byBlockIndices;
        private readonly INoSQLTableStorage<AzureIndex> _byFromIndices;
        private readonly INoSQLTableStorage<AzureIndex> _byToIndices;

        public TransactionReceiptRepository(
            INoSQLTableStorage<TransactionReceiptEntity> transactionReceipts,
            INoSQLTableStorage<AzureIndex> byBlockIndices,
            INoSQLTableStorage<AzureIndex> byFromIndices,
            INoSQLTableStorage<AzureIndex> byToIndices)
        {
            _transactionReceipts = transactionReceipts;
            _byBlockIndices = byBlockIndices;
            _byFromIndices = byFromIndices;
            _byToIndices = byToIndices;
        }

        public static ITransactionReceiptRepository Create(
            IReloadingManager<string> connectionString,
            ILogFactory logFactory)
        {
            var transactionReceipts = AzureTableStorage<TransactionReceiptEntity>.Create
            (
                connectionString,
                "TransactionReceipts",
                logFactory
            );
            
            var byBlockIndices = AzureTableStorage<AzureIndex>.Create
            (
                connectionString,
                "TransactionReceiptsByBlocks",
                logFactory
            );
            
            var byFromIndices = AzureTableStorage<AzureIndex>.Create
            (
                connectionString,
                "TransactionReceiptsByFrom",
                logFactory
            );
            
            var byToIndices = AzureTableStorage<AzureIndex>.Create
            (
                connectionString,
                "TransactionReceiptsByTo",
                logFactory
            );
            
            return new TransactionReceiptRepository
            (
                transactionReceipts: transactionReceipts,
                byBlockIndices: byBlockIndices,
                byFromIndices: byFromIndices,
                byToIndices: byToIndices
            );
        }


        public async Task<string> CreateContinuationTokenAsync(
            string address,
            TransactionDirection direction,
            string afterHash)
        {
            var receiptPartitionKey = GetReceiptPartitionKey(afterHash);
            var receiptsInTransaction = await _transactionReceipts.GetDataAsync(receiptPartitionKey);

            TransactionReceiptEntity firstAddressReceipt;
            string indexAddress;
            string nonIndexAddress;
            
            switch (direction)
            {
                case TransactionDirection.Incoming:
                    firstAddressReceipt = receiptsInTransaction.FirstOrDefault(x => x.To == address);
                    indexAddress = firstAddressReceipt?.To;
                    nonIndexAddress = firstAddressReceipt?.From;
                    break;
                case TransactionDirection.Outgoing:
                    firstAddressReceipt = receiptsInTransaction.FirstOrDefault(x => x.From == address);
                    indexAddress = firstAddressReceipt?.From;
                    nonIndexAddress = firstAddressReceipt?.To;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (firstAddressReceipt != null)
            {
                var (partitionKey, rowKey) = GetByAddressIndexKeys
                (
                    blockNumber: firstAddressReceipt.BlockNumber,
                    indexAddress: indexAddress,
                    nonIndexAddress: nonIndexAddress,
                    hash: firstAddressReceipt.Hash,
                    index: firstAddressReceipt.Index
                );
                
                return JsonConvert.SerializeObject(new TableContinuationToken
                {
                    NextPartitionKey = partitionKey,
                    NextRowKey = rowKey
                }).StringToHex();
            }
            else
            {
                return null;
            }
        }

        public async Task InsertOrReplaceAsync(
            TransactionReceipt receipt)
        {
            var receiptEntityKeys = GetReceiptKeys(receipt);
            var receiptEntity = new TransactionReceiptEntity
            {
                Amount = receipt.Amount,
                BlockNumber = receipt.BlockNumber,
                From = receipt.From,
                Hash = receipt.Hash,
                Index = receipt.Index,
                TransactionTimestamp = receipt.Timestamp,
                To = receipt.To,
                
                PartitionKey = receiptEntityKeys.PartitionKey,
                RowKey = receiptEntityKeys.RowKey
            };
            
            var byFromIndexKeys = GetByFromIndexKeys(receipt);
            var indexByFrom = AzureIndex.Create
            (
                byFromIndexKeys.PartitionKey,
                byFromIndexKeys.RowKey,
                receiptEntity
            );
            
            var byToIndexKeys = GetByToIndexKeys(receipt);
            var byToIndex = AzureIndex.Create
            (
                byToIndexKeys.PartitionKey,
                byToIndexKeys.RowKey,
                receiptEntity
            );
            
            var byBlockIndexKeys = GetByBlockIndexKeys(receipt);
            var byBlockIndex = AzureIndex.Create
            (
                byBlockIndexKeys.PartitionKey,
                byBlockIndexKeys.RowKey,
                receiptEntity
            );


            await Task.WhenAll
            (
                _transactionReceipts.InsertOrReplaceAsync(receiptEntity),
                _byFromIndices.InsertOrReplaceAsync(indexByFrom),
                _byToIndices.InsertOrReplaceAsync(byToIndex),
                _byBlockIndices.InsertOrReplaceAsync(byBlockIndex)
            );
        }

        public async Task<(IEnumerable<TransactionReceipt> Transactions, string ContinuationToken)> GetAsync(
            string address,
            TransactionDirection direction,
            int take,
            string continuationToken)
        {
            INoSQLTableStorage<AzureIndex> byAddressIndices;
            IEnumerable<AzureIndex> indices;

            switch (direction)
            {
                case TransactionDirection.Incoming:
                    byAddressIndices = _byToIndices;
                    break;
                case TransactionDirection.Outgoing:
                    byAddressIndices = _byFromIndices;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            var partitionKey = GetAddressIndexPartitionKey(address);
            
            (indices, continuationToken) = await byAddressIndices
                .GetDataWithContinuationTokenAsync(partitionKey, take, continuationToken);

            var receiptKeys = indices
                .Select(x => new Tuple<string, string>(x.PrimaryPartitionKey, x.PrimaryRowKey));

            var receipts = (await _transactionReceipts.GetDataAsync(receiptKeys))
                .Select(x => new TransactionReceipt
                {
                    Amount = x.Amount,
                    BlockNumber = x.BlockNumber,
                    From = x.From,
                    Timestamp = x.TransactionTimestamp,
                    To = x.To,
                    Hash = x.Hash
                });

            return (receipts, continuationToken);
        }

        public async Task ClearBlockAsync(
            BigInteger blockNumber)
        {
            var byBlockIndexPartitionKey = GetByBlockIndexPartitionKey(blockNumber);
            var receiptsInBlock = await _byBlockIndices.GetDataAsync(byBlockIndexPartitionKey);
            
            foreach (var receiptInBlock in receiptsInBlock)
            {
                var receiptEntity = await _transactionReceipts.GetDataAsync
                (
                    partition: receiptInBlock.PrimaryPartitionKey,
                    row: receiptInBlock.PrimaryRowKey
                );

                if (receiptEntity != null)
                {
                    var byFromIndexKeys = GetByFromIndexKeys(receiptEntity);
                    var byToIndexKeys = GetByToIndexKeys(receiptEntity);
                
                    await Task.WhenAll
                    (
                        _byFromIndices.DeleteIfExistAsync
                        (
                            byFromIndexKeys.PartitionKey,
                            byFromIndexKeys.RowKey
                        ),
                        _byToIndices.DeleteIfExistAsync
                        (
                            byToIndexKeys.PartitionKey,
                            byToIndexKeys.RowKey
                        )
                    );

                    await _transactionReceipts.DeleteIfExistAsync(receiptEntity.PartitionKey, receiptEntity.RowKey);
                }

                await _byBlockIndices.DeleteIfExistAsync(receiptInBlock.PartitionKey, receiptInBlock.RowKey);
            }
        }
        
        #region Key Builders

        private static string FormatNumberForKey(
            BigInteger value)
        {
            return $"{value:00000000000000000000}";
        }
        
        private static (string PartitionKey, string RowKey) GetReceiptKeys(
            TransactionReceipt receipt)
        {
            return (GetReceiptPartitionKey(receipt.Hash), $"{receipt.Hash}-{FormatNumberForKey(receipt.Index)}");
        }
        
        private static string GetReceiptPartitionKey(
            string hash)
        {
            return hash.CalculateHexHash32(3);
        }

        private static (string PartitionKey, string RowKey) GetByBlockIndexKeys(
            TransactionReceipt receipt)
        {
            return (GetByBlockIndexPartitionKey(receipt.BlockNumber), $"{receipt.Hash}-{FormatNumberForKey(receipt.Index)}");
        }
        
        private static string GetByBlockIndexPartitionKey(
            BigInteger blockNumber)
        {
            return FormatNumberForKey(blockNumber);
        }

        private static (string PartitionKey, string RowKey) GetByFromIndexKeys(
            TransactionReceiptEntity receipt)
        {
            return GetByAddressIndexKeys
            (
                blockNumber: receipt.BlockNumber,
                indexAddress: receipt.From,
                nonIndexAddress: receipt.To,
                hash: receipt.Hash,
                index: receipt.Index
            );
        }
        
        private static (string PartitionKey, string RowKey) GetByFromIndexKeys(
            TransactionReceipt receipt)
        {
            return GetByAddressIndexKeys
            (
                blockNumber: receipt.BlockNumber,
                indexAddress: receipt.From,
                nonIndexAddress: receipt.To,
                hash: receipt.Hash,
                index: receipt.Index
            );
        }
        
        private static (string PartitionKey, string RowKey) GetByToIndexKeys(
            TransactionReceiptEntity receipt)
        {
            return GetByAddressIndexKeys
            (
                blockNumber: receipt.BlockNumber,
                indexAddress: receipt.To,
                nonIndexAddress: receipt.From,
                hash: receipt.Hash,
                index: receipt.Index
            );
        }
        
        private static (string PartitionKey, string RowKey) GetByToIndexKeys(
            TransactionReceipt receipt)
        {
            return GetByAddressIndexKeys
            (
                blockNumber: receipt.BlockNumber,
                indexAddress: receipt.To,
                nonIndexAddress: receipt.From,
                hash: receipt.Hash,
                index: receipt.Index
            );
        }
        
        private static (string PartitionKey, string RowKey) GetByAddressIndexKeys(
            BigInteger blockNumber,
            string indexAddress,
            string nonIndexAddress,
            string hash,
            BigInteger index)
        {
            return 
            (
                GetAddressIndexPartitionKey(indexAddress),
                $"{FormatNumberForKey(blockNumber)}-{nonIndexAddress ?? "null"}-{hash}-{FormatNumberForKey(index)}"
            );
        }

        private static string GetAddressIndexPartitionKey(
            string address)
        {
            return address ?? "null";
        }

        
        
        #endregion
    }
}

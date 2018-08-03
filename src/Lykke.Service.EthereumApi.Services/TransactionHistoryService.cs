using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;

namespace Lykke.Service.EthereumApi.Services
{
    [UsedImplicitly]
    public class TransactionHistoryService : ITransactionHistoryService
    {
        private readonly ITransactionReceiptRepository _transfactionReceiptRepository;

        
        public TransactionHistoryService(
            ITransactionReceiptRepository transfactionReceiptRepository)
        {
            _transfactionReceiptRepository = transfactionReceiptRepository;
        }

        
        public Task<IEnumerable<TransactionReceipt>> GetIncomingHistoryAsync(
            string address,
            int take,
            string afterHash)
        {
            return GetHistoryAsync
            (
                address,
                TransactionDirection.Incoming,
                take,
                afterHash
            );
        }

        public Task<IEnumerable<TransactionReceipt>> GetOutgoingHistoryAsync(
            string address,
            int take,
            string afterHash)
        {
            return GetHistoryAsync
            (
                address,
                TransactionDirection.Outgoing,
                take,
                afterHash
            );
        }

        private async Task<IEnumerable<TransactionReceipt>> GetHistoryAsync(
            string address,
            TransactionDirection transactionDirection,
            int take,
            string afterHash)
        {
            var continuationToken = await _transfactionReceiptRepository.CreateContinuationTokenAsync
            (
                address,
                transactionDirection,
                afterHash
            );

            var (transactionReceipts, _) = await _transfactionReceiptRepository.GetAsync
            (
                address,
                transactionDirection,
                take,
                continuationToken
            );

            return transactionReceipts;
        }
        
        
    }
}

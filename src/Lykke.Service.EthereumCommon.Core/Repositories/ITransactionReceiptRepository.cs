using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Domain;

namespace Lykke.Service.EthereumCommon.Core.Repositories
{
    public interface ITransactionReceiptRepository
    {
        Task<string> CreateContinuationTokenAsync(
            [NotNull] string address,
            TransactionDirection direction,
            string afterHash);
        
        
        Task InsertOrReplaceAsync(
            [NotNull] TransactionReceipt receipt);

        Task<(IEnumerable<TransactionReceipt> Transactions, string ContinuationToken)> GetAsync(
            [NotNull] string address,
            TransactionDirection direction,
            int take,
            [CanBeNull] string continuationToken);

        Task ClearBlockAsync(
            BigInteger blockNumber);
    }
}

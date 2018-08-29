using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Domain;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface ITransactionHistoryService
    {
        [ItemNotNull]
        Task<IEnumerable<TransactionReceipt>> GetIncomingHistoryAsync(
            [NotNull] string address,
            int take,
            string afterHash);
        
        [ItemNotNull]
        Task<IEnumerable<TransactionReceipt>> GetOutgoingHistoryAsync(
            [NotNull] string address,
            int take,
            string afterHash);
    }
}

using System;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Domain;

namespace Lykke.Service.EthereumApi.Core.Services
{
    public interface ITransactionService
    {
        [ItemNotNull]
        Task<BuildTransactionResult> BuildTransactionAsync(
            Guid transactionId,
            [NotNull] string from,
            [NotNull] string to,
            BigInteger amount);

        [ItemNotNull]
        Task<BroadcastTransactionResult> BroadcastTransactionAsync(
            Guid transactionId,
            [NotNull] string signedTxData);

        Task<bool> DeleteTransactionIfExistsAsync(
            Guid transactionId);
        
        Task<Transaction> TryGetTransactionAsync(
            Guid transactionId);
    }
}

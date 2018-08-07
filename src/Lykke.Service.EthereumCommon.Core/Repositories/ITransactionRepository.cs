using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Domain;

namespace Lykke.Service.EthereumCommon.Core.Repositories
{
    public interface ITransactionRepository
    {
        Task AddAsync(
            [NotNull] Transaction transaction);

        [ItemCanBeNull]
        Task<Transaction> TryGetAsync(
            Guid transactionId);

        Task UpdateAsync(
            [NotNull] Transaction transaction);
    }
}

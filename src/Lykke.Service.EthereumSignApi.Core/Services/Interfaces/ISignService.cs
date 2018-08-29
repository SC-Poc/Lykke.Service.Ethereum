using JetBrains.Annotations;

namespace Lykke.Service.EthereumSignApi.Core.Services.Interfaces
{
    public interface ISignService
    {
        [Pure, NotNull]
        string SignTransaction(
            [NotNull] string transactionContext,
            [NotNull] string privateKey);
    }
}

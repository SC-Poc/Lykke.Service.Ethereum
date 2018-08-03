using System;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Core.Services;


namespace Lykke.Service.EthereumApi.Services
{
    [UsedImplicitly]
    public class BlockchainService : IBlockchainService
    {
        public Task<BigInteger> GetBalanceAsync(
            string address)
        {
            throw new NotImplementedException();
        }

        public Task<string> BroadcastTransactionAsync(
            string signedTxData)
        {
            throw new NotImplementedException();
        }

        public Task<string> BuildTransactionAsync(
            string to,
            BigInteger amount)
        {
            throw new NotImplementedException();
        }
    }
}

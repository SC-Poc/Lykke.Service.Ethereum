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
        public async Task<BigInteger> GetBalanceAsync(
            string address)
        {
            throw new NotImplementedException();
        }

        public async Task<string> BroadcastTransactionAsync(
            string signedTxData)
        {
            throw new NotImplementedException();
        }

        public async Task<string> BuildTransactionAsync(
            string to,
            BigInteger amount)
        {
            throw new NotImplementedException();
        }
    }
}

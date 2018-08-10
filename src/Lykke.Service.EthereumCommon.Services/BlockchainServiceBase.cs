using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Nethereum.Parity;
using Nethereum.Web3;

namespace Lykke.Service.EthereumCommon.Services
{
    public abstract class BlockchainServiceBase
    {
        private readonly SemaphoreSlim _bestBlockSemaphore;
        
        private DateTime _bestBlockInfoExpiration;
        private BigInteger _bestTrustedBlockNumber;
        
        protected readonly int ConfirmationLevel;
        protected readonly ILog Log;
        protected readonly Web3Parity Web3;
        
        
        protected BlockchainServiceBase(
            int confirmationLevel,
            ILogFactory logFactory,
            Web3Parity web3)
        {
            _bestBlockSemaphore = new SemaphoreSlim(1);
            
            ConfirmationLevel = confirmationLevel;
            Log = logFactory.CreateLog(this);
            Web3 = web3;
        }
        
        public async Task<BigInteger> GetBestTrustedBlockNumberAsync()
        {
            await UpdateBestBlockAsync();

            return _bestTrustedBlockNumber;
        }

        private async Task UpdateBestBlockAsync()
        {
            await _bestBlockSemaphore.WaitAsync();

            try
            {
                if (DateTime.UtcNow <= _bestBlockInfoExpiration)
                {
                    var bestBlockNumber = (await Web3.Eth.Blocks.GetBlockNumber.SendRequestAsync()).Value;

                    _bestTrustedBlockNumber = bestBlockNumber - ConfirmationLevel;
                    
                    _bestBlockInfoExpiration = DateTime.UtcNow.AddSeconds(5);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to update best block.");
            }
            finally
            {
                _bestBlockSemaphore.Release();
            }
        }
    }
}

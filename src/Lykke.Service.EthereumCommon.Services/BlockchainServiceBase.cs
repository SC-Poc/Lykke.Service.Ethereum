using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Service.EthereumCommon.Services
{
    public abstract class BlockchainServiceBase
    {
        private readonly SemaphoreSlim _bestBlockSemaphore;
        

        private object _bestBlockInfo;
        private DateTime _bestBlockInfoExpiration;
        
        protected readonly int ConfirmationLevel;
        
        
        protected BlockchainServiceBase(
            int confirmationLevel)
        {
            _bestBlockSemaphore = new SemaphoreSlim(1);
            
            ConfirmationLevel = confirmationLevel;
        }
        
        public async Task<BigInteger> GetBestTrustedBlockNumberAsync()
        {
            await UpdateBestBlockAsync();
            
            throw new NotImplementedException();
            
            //return _bestBlockInfo.Number - ConfirmationLevel;
        }

        private async Task UpdateBestBlockAsync()
        {
            await _bestBlockSemaphore.WaitAsync();

            try
            {
                if (DateTime.UtcNow <= _bestBlockInfoExpiration)
                {
                    throw new NotImplementedException();
                    
                    //_bestBlockInfo = await Blockchain.TryGetBlockAsync(BlockRevision.Best);
                    
                    _bestBlockInfoExpiration = DateTime.UtcNow.AddSeconds(5);
                }
            }
            catch (Exception)
            {
                // TODO: Log error
            }
            finally
            {
                _bestBlockSemaphore.Release();
            }
        }
    }
}

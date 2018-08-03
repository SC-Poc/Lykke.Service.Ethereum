using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Services;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.Service.EthereumWorker.Core.Services;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Lykke.Service.EthereumWorker.Services
{
    [UsedImplicitly]
    public class BlockchainService : BlockchainServiceBase, IBlockchainService
    {
        public BlockchainService(
            int confirmationLevel) 
            : base(confirmationLevel)
        {
            
        }


        public async Task<BigInteger> GetBalanceAsync(string address, BigInteger blockNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<TransfactionResult> GetTransactionResultAsync(string hash)
        {
            throw new NotImplementedException();
        }
    }
}

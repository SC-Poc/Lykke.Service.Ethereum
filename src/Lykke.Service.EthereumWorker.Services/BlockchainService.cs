using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.Services;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.Service.EthereumWorker.Core.Services;
using Nethereum.JsonRpc.Client;
using Nethereum.Parity;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using TransactionReceipt = Lykke.Service.EthereumCommon.Core.Domain.TransactionReceipt;


namespace Lykke.Service.EthereumWorker.Services
{
    [UsedImplicitly]
    public class BlockchainService : BlockchainServiceBase, IBlockchainService
    {
        public BlockchainService(
            ILogFactory logFactory,
            Settings settings,
            Web3Parity web3)
            : base(settings.ConfirmationLevel, logFactory, web3)
        {
            
        }


        public async Task<BigInteger> GetBalanceAsync(
            string address,
            BigInteger blockNumber)
        {
            try
            {
                var block = new BlockParameter((ulong) blockNumber);
                var balance = await Web3.Eth.GetBalance.SendRequestAsync(address, block);

                return balance.Value;
            }
            catch (RpcResponseException e) when (e.RpcError.Code == -32602)
            {
                throw new ArgumentOutOfRangeException("Block number is too high.", e);
            }
        }

        public Task<TransfactionResult> GetTransactionResultAsync(
            string hash)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TransactionReceipt>> GetTransactionReceiptsAsync(
            BigInteger blockNumbber)
        {
            throw new NotImplementedException();
        }


        public class Settings
        {
            public int ConfirmationLevel { get; set; }
        }
    }
}

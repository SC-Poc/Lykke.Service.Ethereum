using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.Service.EthereumWorker.Core.Services;
using Lykke.Service.EthereumWorker.Services.Models;
using Nethereum.JsonRpc.Client;
using Nethereum.Parity;
using Nethereum.RPC.Eth.DTOs;

using TransactionReceipt = Lykke.Service.EthereumCommon.Core.Domain.TransactionReceipt;


namespace Lykke.Service.EthereumWorker.Services
{
    [UsedImplicitly]
    public class BlockchainService : IBlockchainService
    {
        private readonly SemaphoreSlim _bestTrustedBlockLock;
        private readonly int _confirmationLevel;
        private readonly Web3Parity _web3;
        
        
        private DateTime _bestTrustedBlockExpiration;
        private BigInteger _bestTrustedBlockNumber;
        
        
        public BlockchainService(
            ILogFactory logFactory,
            Settings settings,
            Web3Parity web3)
        {
            _bestTrustedBlockLock = new SemaphoreSlim(1);
            _confirmationLevel = settings.ConfirmationLevel;
            _web3 = web3;
        }


        public async Task<BigInteger> GetBalanceAsync(
            string address,
            BigInteger blockNumber)
        {
            try
            {
                var block = new BlockParameter((ulong) blockNumber);
                var balance = await _web3.Eth.GetBalance.SendRequestAsync(address, block);

                return balance.Value;
            }
            catch (RpcResponseException e) when (e.RpcError.Code == -32602)
            {
                throw new ArgumentOutOfRangeException("Block number is too high.", e);
            }
        }

        public async Task<BigInteger> GetBestTrustedBlockNumberAsync()
        {
            if (_bestTrustedBlockExpiration <= DateTime.UtcNow)
            {
                await _bestTrustedBlockLock.WaitAsync();

                try
                {
                    if (_bestTrustedBlockExpiration <= DateTime.UtcNow)
                    {
                        var bestBlockNumber = (await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync()).Value;
                        
                        _bestTrustedBlockNumber =  bestBlockNumber - _confirmationLevel;
                        _bestTrustedBlockExpiration = DateTime.UtcNow.AddSeconds(5);
                    }
                }
                finally
                {
                    _bestTrustedBlockLock.Release();
                }
            }

            return _bestTrustedBlockNumber;
        }

        public async Task<TransfactionResult> GetTransactionResultAsync(
            string hash)
        {
            var traces = await GetTransactionTracesAsync(hash);

            if (traces.Any())
            {
                return new TransfactionResult
                {
                    BlockNumber = traces.First().BlockNumber,
                    IsCompleted = true,
                    IsFailed = traces.Any(x => !string.IsNullOrEmpty(x.Error))
                };
            }
            else
            {
                return new TransfactionResult
                {
                    IsCompleted = false,
                    IsFailed = false
                };
            }
        }

        public Task<IEnumerable<TransactionReceipt>> GetTransactionReceiptsAsync(
            BigInteger blockNumbber)
        {
            throw new NotImplementedException();
        }
        
        private async Task<TransactionTraceResponse[]> GetTransactionTracesAsync(string txHash)
        {
            var request = new RpcRequest($"{Guid.NewGuid()}", "trace_transaction", txHash);
            var response = await _web3.Client.SendRequestAsync<IEnumerable<TransactionTraceResponse>>(request);

            if (response != null)
            {
                return response.ToArray();
            }
            else
            {
                return Array.Empty<TransactionTraceResponse>();
            }
        }
        

        public class Settings
        {
            public int ConfirmationLevel { get; set; }
        }
    }
}

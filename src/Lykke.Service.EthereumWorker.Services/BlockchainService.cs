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
using Nethereum.Hex.HexTypes;
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
                        _bestTrustedBlockExpiration = DateTime.UtcNow.AddSeconds(30);
                    }
                }
                finally
                {
                    _bestTrustedBlockLock.Release();
                }
            }

            return _bestTrustedBlockNumber;
        }

        public async Task<TransactionResult> GetTransactionResultAsync(
            string hash)
        {
            var traces = await GetTransactionTracesAsync(hash);

            if (traces.Any())
            {
                var error = string.Join(";", traces
                    .Where(x => !string.IsNullOrEmpty(x.Error))
                    .Select(x => x.Error));
                
                return new TransactionResult
                {
                    BlockNumber = traces.First().BlockNumber,
                    Error = error,
                    IsCompleted = true,
                    IsFailed = !string.IsNullOrEmpty(error)
                };
            }
            else
            {
                return new TransactionResult
                {
                    IsCompleted = false,
                    IsFailed = false
                };
            }
        }

        public async Task<IEnumerable<TransactionReceipt>> GetTransactionReceiptsAsync(
            BigInteger blockNumber)
        {
            var block = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(blockNumber));

            if (block != null)
            {
                return block.Transactions.Where(x => x.Value.Value != 0).Select(x => new TransactionReceipt
                {
                    Amount = x.Value.Value,
                    BlockNumber = blockNumber,
                    From = x.From,
                    Hash = x.TransactionHash,
                    Timestamp = block.Timestamp,
                    To = x.To
                });
            }

            return Enumerable.Empty<TransactionReceipt>();
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

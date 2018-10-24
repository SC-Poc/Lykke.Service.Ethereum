using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Services;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.Service.EthereumWorker.Core.Services;
using Lykke.Service.EthereumWorker.Services.Models;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.RPC.Eth.DTOs;

using TransactionReceipt = Lykke.Service.EthereumCommon.Core.Domain.TransactionReceipt;


namespace Lykke.Service.EthereumWorker.Services
{
    [UsedImplicitly]
    public class BlockchainService : BlockchainServiceBase, IBlockchainService
    {
        private readonly int _confirmationLevel;

        private readonly string[] _valueTransferCallCodes =
            new[] {"CREATE", "CALL", "CALLCODE", "DELEGATECALL", "SUICIDE"};
        
        
        public BlockchainService(
            Settings settings)
        
            : base(settings.ParityNodeUrl)
        {
            _confirmationLevel = settings.ConfirmationLevel;
        }


        public async Task<BigInteger> GetBalanceAsync(
            string address,
            BigInteger blockNumber)
        {
            try
            {
                var block = new BlockParameter((ulong) blockNumber);
                var balance = await SendRequestWithTelemetryAsync<HexBigInteger>
                (
                    Web3.Eth.GetBalance.BuildRequest(address, block)
                );

                return balance.Value;
            }
            catch (RpcResponseException e) when (e.RpcError.Code == -32602)
            {
                throw new ArgumentOutOfRangeException("Block number is too high.", e);
            }
        }

        public async Task<BigInteger> GetBestTrustedBlockNumberAsync()
        {
            var bestBlockNumber = await SendRequestWithTelemetryAsync<HexBigInteger>
            (
                Web3.Eth.Blocks.GetBlockNumber.BuildRequest()
            );

            return bestBlockNumber.Value - _confirmationLevel;
        }

        public async Task<TransactionResult> GetTransactionResultAsync(
            string hash)
        {
            #if ETH
            
            var receipt = await SendRequestWithTelemetryAsync<Nethereum.RPC.Eth.DTOs.TransactionReceipt>
            (
                Web3.Eth.Transactions.GetTransactionReceipt.BuildRequest(hash)
            );

            if (receipt != null && receipt.BlockNumber.Value != 0)
            {
                var isFailed = receipt.Status.Value == 0;
                var error = isFailed ? "Transaction failed." : null;
                
                return new TransactionResult
                {
                    BlockNumber = receipt.BlockNumber.Value,
                    Error = error,
                    IsCompleted = true,
                    IsFailed = isFailed
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
            
            #elif ETC

            throw new NotImplementedException();
    
            #endif
        }

        public async Task<IEnumerable<TransactionReceipt>> GetTransactionReceiptsAsync(
            BigInteger blockNumber)
        {
            var block = await SendRequestWithTelemetryAsync<BlockWithTransactions>
            (
                Web3.Eth.Blocks.GetBlockWithTransactionsByNumber.BuildRequest
                (
                    new HexBigInteger(blockNumber)
                )
            );
            
            if (block != null)
            {
                var result = new List<TransactionReceipt>();
                
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.To != null && !await IsWalletAsync(transaction.To))
                    {
                        result.AddRange
                        (
                            await GetInternalTransactionReceiptsAsync
                            (
                                transaction.TransactionHash,
                                block.Timestamp
                            )
                        );
                    }
                    else
                    {
                        result.Add(new TransactionReceipt
                        {
                            Amount = transaction.Value.Value,
                            BlockNumber = blockNumber,
                            From = transaction.From,
                            Hash = transaction.TransactionHash,
                            Index = 0,
                            Timestamp = block.Timestamp,
                            To = transaction.To
                        });
                    }
                }

                return result;
            }
            else
            {
                return Enumerable.Empty<TransactionReceipt>();
            }
        }

        private async Task<IEnumerable<TransactionReceipt>> GetInternalTransactionReceiptsAsync(
            string txHash,
            BigInteger timestamp)
        {
            var traces = await GetTransactionTracesAsync(txHash);

            if (traces != null && traces.Length > 0)
            {
                var result = new List<TransactionReceipt>();

                var valueTransferTraces = traces
                    .Where(x => _valueTransferCallCodes.Contains(x.Action.CallType, StringComparer.InvariantCultureIgnoreCase))
                    .Where(x => x.Action.Value != "0x0");

                var index = 0;
                
                foreach (var trace in valueTransferTraces)
                {
                    var amount = new HexBigInteger(trace.Action.Value).Value;
                    
                    result.Add(new TransactionReceipt
                    {
                        Amount = amount,
                        BlockNumber = trace.BlockNumber,
                        From = trace.Action.From,
                        Hash = trace.TransactionHash,
                        Index = index++,
                        Timestamp = timestamp,
                        To = trace.Action.To
                    });
                }

                return result;
            }
            else
            {
                return Enumerable.Empty<TransactionReceipt>();
            }
        }
        
        
        private async Task<TransactionTraceResponse[]> GetTransactionTracesAsync(string txHash)
        {
            var transactionTraces = await SendRequestWithTelemetryAsync<IEnumerable<TransactionTraceResponse>>
            (
                new RpcRequest(Guid.NewGuid(), "trace_transaction", txHash)
            );
        
            if (transactionTraces != null)
            {
                return transactionTraces.ToArray();
            }
            else
            {
                return Array.Empty<TransactionTraceResponse>();
            }
        }
        
        public class Settings
        {
            public int ConfirmationLevel { get; set; }
            
            public string ParityNodeUrl { get; set; }
        }
    }
}

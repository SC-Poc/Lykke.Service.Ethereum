using System;
using System.Numerics;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumCommon.Core.Domain;
using MessagePack;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Hex.HexTypes;
using Nethereum.JsonRpc.Client;
using Nethereum.Parity;
using Nethereum.RPC.Eth.DTOs;


namespace Lykke.Service.EthereumApi.Services
{
    [UsedImplicitly]
    public class BlockchainService : IBlockchainService
    {
        private readonly ILog _log;
        private readonly Web3Parity _web3;

        public BlockchainService(
            ILogFactory logFactory,
            Web3Parity web3)
        {
            _log = logFactory.CreateLog(this);
            _web3 = web3;
        }


        public async Task<BigInteger> GetBalanceAsync(
            string address)
        {
            return (await _web3.Eth.GetBalance.SendRequestAsync(address))
                .Value;
        }

        public async Task<string> BroadcastTransactionAsync(
            string signedTxData)
        {
            var txHash = GetTransactionHash(signedTxData);
            var txReceipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash);

            if (txReceipt == null)
            {
                await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTxData);
                
                for (var i = 0; i < 10; i++)
                {
                    if (await CheckIfBroadcastedAsync(txHash))
                    {
                        return txHash;
                    }
                    else
                    {
                        await Task.Delay(1000);
                    }
                }
                
                throw new Exception
                (
                    $"Transaction [{txHash}] has been broadcasted, but did not appear in mempol during the specified period of time."
                );
            }
            
            return txHash;
        }

        public async Task<string> BuildTransactionAsync(
            string from,
            string to,
            BigInteger amount)
        {
            var transaction = new UnsignedTransaction
            {
                Amount = amount,
                GasAmount = 21000,
                GasPrice = await EstimateGasPriceAsync(to, amount),
                Nonce = await GetNextNonceAsync(from),
                To = to
            };

            return MessagePackSerializer
                .Serialize(transaction)
                .ToHex(prefix: true);
        }

        public async Task<bool> IsWalletAsync(
            string address)
        {
            var code = await _web3.Eth.GetCode.SendRequestAsync(address);

            return code == "0x";
        }
        
        private async Task<bool> CheckIfBroadcastedAsync(
            string txHash)
        {
            var transaction = await _web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(txHash);

            return transaction != null && transaction.BlockNumber.Value == 0;
        }
        
        private async Task<BigInteger> EstimateGasPriceAsync(
            string to,
            BigInteger amount)
        {   
            var input = new TransactionInput
            {
                To = to,
                Value = new HexBigInteger(amount)
            };

            return (await _web3.Eth.Transactions.EstimateGas.SendRequestAsync(input))
                .Value;
        }
        
        private async Task<BigInteger> GetNextNonceAsync(
            string address)
        {   
            var request = new RpcRequest($"{Guid.NewGuid()}", "parity_nextNonce", address);
            var response = await _web3.Client.SendRequestAsync<string>(request);
            var result = new HexBigInteger(response);

            return result.Value;
        }

        private static string GetTransactionHash(
            string signedTxData)
        {
            var txDataBytes = signedTxData.HexToByteArray();
            
            return (new Nethereum.Signer.Transaction(txDataBytes)).RawHash
                .ToHex(true);
        }
    }
}

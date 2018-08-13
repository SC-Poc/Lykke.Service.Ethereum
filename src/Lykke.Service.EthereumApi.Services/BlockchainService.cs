using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.SettingsReader;
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
        private readonly SemaphoreSlim _gasPriceLock;
        private readonly ILog _log;
        private readonly IReloadingManager<string> _maxGasPriceManager;
        private readonly IReloadingManager<string> _minGasPriceManager;
        private readonly Web3Parity _web3;

        private DateTime _gasPriceExpiration;
        
        
        public BlockchainService(
            ILogFactory logFactory,
            Settings settings,
            Web3Parity web3)
        {
            _gasPriceLock = new SemaphoreSlim(1);
            _log = logFactory.CreateLog(this);
            _maxGasPriceManager = settings.MaxGasPriceManager;
            _minGasPriceManager = settings.MinGasPriceManager;
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
                    $"Transaction [{txHash}] has been broadcasted, but did not appear in mempool during the specified period of time."
                );
            }
            
            return txHash;
        }

        public async Task<string> BuildTransactionAsync(
            string from,
            string to,
            BigInteger amount,
            BigInteger gasPrice)
        {
            var transaction = new UnsignedTransaction
            {
                Amount = amount,
                GasAmount = Constants.GasAmount,
                GasPrice = gasPrice,
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
        
        public async Task<BigInteger> EstimateGasPriceAsync(
            string to,
            BigInteger amount)
        {   
            await UpdateMinAndMaxGasPricesAsync();

            var input = new TransactionInput
            {
                To = to,
                Value = new HexBigInteger(amount)
            };
            
            var estimatedGasPrice = (await _web3.Eth.Transactions.EstimateGas.SendRequestAsync(input)).Value;
            var minGasPrice = BigInteger.Parse(_minGasPriceManager.CurrentValue);
            var maxGasPrice = BigInteger.Parse(_maxGasPriceManager.CurrentValue);

            if (estimatedGasPrice > maxGasPrice)
            {
                return maxGasPrice;
            }

            if (estimatedGasPrice < minGasPrice)
            {
                return minGasPrice;
            }

            return estimatedGasPrice;
        }
        
        private async Task<BigInteger> GetNextNonceAsync(
            string address)
        {   
            var request = new RpcRequest($"{Guid.NewGuid()}", "parity_nextNonce", address);
            var response = await _web3.Client.SendRequestAsync<string>(request);
            var result = new HexBigInteger(response);

            return result.Value;
        }

        private async Task UpdateMinAndMaxGasPricesAsync()
        {
            if (_gasPriceExpiration <= DateTime.UtcNow)
            {
                await _gasPriceLock.WaitAsync();

                try
                {
                    var previousMaxGasPrice = _maxGasPriceManager.CurrentValue;
                    var previousMinGasPrice = _minGasPriceManager.CurrentValue;
                    
                    if (_gasPriceExpiration <= DateTime.UtcNow)
                    {
                        await Task.WhenAll
                        (
                            _maxGasPriceManager.Reload(),
                            _minGasPriceManager.Reload()
                        );

                        _gasPriceExpiration = DateTime.UtcNow.AddMinutes(1);
                    }
                    
                    var newMaxGasPrice = _maxGasPriceManager.CurrentValue;
                    var newMinGasPrice = _minGasPriceManager.CurrentValue;

                    if (newMaxGasPrice != previousMaxGasPrice)
                    {
                        _log.Info($"Maximal gas price seto to {newMaxGasPrice}");
                    }
                    
                    if (newMinGasPrice != previousMinGasPrice)
                    {
                        _log.Info($"Maximal gas price seto to {newMinGasPrice}");
                    }
                }
                catch (Exception e)
                {
                    _log.Error(e, "Failed to update minimal and maximal gas prices.");
                }
                finally
                {
                    _gasPriceLock.Release();
                }
            }
        }

        private static string GetTransactionHash(
            string signedTxData)
        {
            var txDataBytes = signedTxData.HexToByteArray();
            
            return (new Nethereum.Signer.Transaction(txDataBytes)).RawHash
                .ToHex(true);
        }


        public class Settings
        {
            public IReloadingManager<string> MaxGasPriceManager { get; set; }
            public IReloadingManager<string> MinGasPriceManager { get; set; }
        }
    }
}

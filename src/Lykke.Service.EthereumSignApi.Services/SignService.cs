using System;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Telemetry;
using Lykke.Service.EthereumSignApi.Core.Services.Interfaces;
using MessagePack;
using Microsoft.ApplicationInsights;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;

using NethereumTransaction = Nethereum.Signer.Transaction;


namespace Lykke.Service.EthereumSignApi.Services
{
    [UsedImplicitly]
    public class SignService : ISignService
    {
        private readonly TelemetryClient _telemetryClient;

        
        public SignService()
        {
            _telemetryClient = new TelemetryClient();
        }
        
        
        public string SignTransaction(
            string transactionContext,
            string privateKey)
        {
            using (var eventHolder = _telemetryClient.StartEvent("TransactionSigned"))
            {
                try
                {
                    var transactionBytes = transactionContext.HexToByteArray();
                    var transactionDto = MessagePackSerializer.Deserialize<UnsignedTransaction>(transactionBytes);

                    var transaction = new NethereumTransaction
                    (
                        to: transactionDto.To,
                        amount: transactionDto.Amount,
                        nonce: transactionDto.Nonce,
                        gasPrice: transactionDto.GasPrice,
                        gasLimit: transactionDto.GasAmount,
                        data: null
                    );
            
                    transaction.Sign
                    (
                        key: new EthECKey(privateKey)
                    );

                    return transaction
                        .GetRLPEncoded()
                        .ToHex(prefix: true);
                }
                catch (Exception)
                {
                    eventHolder.TrackFailure("TransactionSigningFailed");

                    throw;
                }
            }
        }
    }
}

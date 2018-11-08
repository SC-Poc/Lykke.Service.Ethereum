using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Nethereum.JsonRpc.Client;
using Nethereum.Parity;

namespace Lykke.Service.EthereumCommon.Services
{
    public abstract class BlockchainServiceBase
    {
        private readonly string _parityNodeUrl;
        private readonly TelemetryClient _telemetryClient;
        
        protected readonly Web3Parity Web3;
        
        protected BlockchainServiceBase(
            string parityNodeUrl)
        {
            _parityNodeUrl = parityNodeUrl;
            _telemetryClient = new TelemetryClient();
            
            Web3 = new Web3Parity(parityNodeUrl);
        }
        
        
        public async Task<bool> IsWalletAsync(
            string address)
        {
            var code = await SendRequestWithTelemetryAsync<string>
            (
                Web3.Eth.GetCode.BuildRequest(Guid.NewGuid(), address, "latest")
            );
            
            return code == "0x";
        }
        
        protected async Task<T> SendRequestWithTelemetryAsync<T>(
            RpcRequest request)
        {
            var operationHolder = StartOperation(request);

            try
            {
                return await Web3.Client.SendRequestAsync<T>(request);
            }
            catch (Exception)
            {
                operationHolder.Telemetry.Success = false;

                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(operationHolder);
            }
        }
        
        protected async Task SendRequestWithTelemetryAsync(
            RpcRequest request)
        {
            var operationHolder = StartOperation(request);

            try
            {
                await Web3.Client.SendRequestAsync(request);
            }
            catch (Exception)
            {
                operationHolder.Telemetry.Success = false;

                throw;
            }
            finally
            {
                _telemetryClient.StopOperation(operationHolder);
            }
        }

        private IOperationHolder<DependencyTelemetry> StartOperation(
            RpcRequest request)
        {
            var operationHolder = _telemetryClient.StartOperation<DependencyTelemetry>(request.Method);

            operationHolder.Telemetry.Data = $"[{string.Join(",", request.RawParameters.Select(x => x.ToString()))}]";
            operationHolder.Telemetry.Target = _parityNodeUrl;
            operationHolder.Telemetry.Type = "Parity";

            return operationHolder;
        }
    }
}

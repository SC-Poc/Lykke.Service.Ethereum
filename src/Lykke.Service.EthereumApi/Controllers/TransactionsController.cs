﻿using System;
using System.Numerics;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.EthereumApi.Core.Domain;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumApi.Models;
using Lykke.Service.EthereumCommon.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Lykke.Service.EthereumApi.Controllers
{
    [PublicAPI, Route("api/transactions")]
    public class TransactionsController : Controller
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(
            ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
        
        [HttpPost("single")]
        public async Task<ActionResult<BuildTransactionResponse>> Build(
            [FromBody] BuildSingleTransactionRequest request)
        {
            var buildResult = await _transactionService.BuildTransactionAsync
            (
                operationId: request.OperationId,
                from: request.FromAddress,
                to: request.ToAddress,
                amount: BigInteger.Parse(request.Amount)
            );

            if (buildResult is BuildTransactionResult.TransactionContext txContext)
            {
                return new BuildTransactionResponse
                {
                    TransactionContext = txContext.String
                };
            }
            else if (buildResult is BuildTransactionResult.Error error)
            {
                switch (error.Type)
                {
                    case BuildTransactionError.AmountIsTooSmall:
                        return BadRequest(
                            BlockchainErrorResponse.FromKnownError(
                                BlockchainErrorCode.AmountIsTooSmall));
                        
                    case BuildTransactionError.BalanceIsNotEnough:
                        return BadRequest(
                            BlockchainErrorResponse.FromKnownError(
                                BlockchainErrorCode.NotEnoughBalance));
                    
                    case BuildTransactionError.TransactionHasBeenBroadcasted:
                        return Conflict(
                            BlockchainErrorResponse.FromUnknownError
                                ($"Transaction for specified operation [{request.OperationId}] has already been broadcasted."));
                    
                    case BuildTransactionError.TransactionHasBeenDeleted:
                        return Conflict(
                            BlockchainErrorResponse.FromUnknownError
                                ($"Transaction for specified operation [{request.OperationId}] has already been deleted."));
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                throw new NotSupportedException(
                    $"{nameof(_transactionService.BuildTransactionAsync)} returned unsupported result.");
            }
        }

        [HttpPost("broadcast")]
        public async Task<ActionResult> Broadcast(
            [FromBody] BroadcastTransactionRequest request)
        {
            var broadcastResult = await _transactionService.BroadcastTransactionAsync
            (
                request.OperationId,
                request.SignedTransaction
            );

            if (broadcastResult is BroadcastTransactionResult.TransactionHash)
            {
                return Ok();
            }
            else if (broadcastResult is BroadcastTransactionResult.Error error)
            {
                switch (error.Type)
                {
                    case BroadcastTransactionError.AmountIsTooSmall:
                        return BadRequest(
                            BlockchainErrorResponse.FromKnownError(
                                BlockchainErrorCode.AmountIsTooSmall));
                    
                    case BroadcastTransactionError.BalanceIsNotEnough:
                        return BadRequest(
                            BlockchainErrorResponse.FromKnownError(
                                BlockchainErrorCode.NotEnoughBalance));
                    
                    case BroadcastTransactionError.TransactionHasBeenBroadcasted:
                        return Conflict(
                            BlockchainErrorResponse.FromUnknownError
                                ($"Transaction for specified operation [{request.OperationId}] has already been broadcasted."));
                    
                    case BroadcastTransactionError.TransactionHasBeenDeleted:
                        return Conflict(
                            BlockchainErrorResponse.FromUnknownError
                                ($"Transaction for specified operation [{request.OperationId}] has already been deleted."));
                    
                    case BroadcastTransactionError.TransactionShouldBeRebuilt:
                        return BadRequest(
                            BlockchainErrorResponse.FromKnownError(
                                BlockchainErrorCode.BuildingShouldBeRepeated));
                    
                    case BroadcastTransactionError.OperationHasNotBeenFound:
                        return NoContent();
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                throw new NotSupportedException(
                    $"{nameof(_transactionService.BroadcastTransactionAsync)} returned unsupported result.");
            }
        }

        [HttpGet("broadcast/single/{operationId:guid}")]
        public async Task<ActionResult<BroadcastedSingleTransactionResponse>> GetSingleTransactionState(
            OperationRequest request)
        {
            var txState = await _transactionService.TryGetTransactionAsync
            (
                request.OperationId
            );

            if (txState != null)
            {
                var response = new BroadcastedSingleTransactionResponse();
                
                switch (txState.State)
                {
                    case TransactionState.Built:
                        return NoContent();
                    case TransactionState.InProgress:
                        response.State = BroadcastedTransactionState.InProgress;
                        response.Timestamp = txState.BuiltOn;
                        break;
                    case TransactionState.Completed:
                        response.State = BroadcastedTransactionState.Completed;
                        // ReSharper disable once PossibleInvalidOperationException
                        response.Timestamp = txState.CompletedOn.Value;
                        break;
                    case TransactionState.Failed:
                        response.State = BroadcastedTransactionState.Failed;
                        // ReSharper disable once PossibleInvalidOperationException
                        response.Timestamp = txState.CompletedOn.Value;
                        break;
                    case TransactionState.Deleted:
                        return NoContent();
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (txState.BlockNumber.HasValue)
                {
                    response.Block = (long) txState.BlockNumber.Value;
                }

                if (!string.IsNullOrEmpty(txState.Error))
                {
                    response.Error = txState.Error;
                    response.ErrorCode = BlockchainErrorCode.Unknown;
                }
                
                return response;
            }
            else
            {
                return NoContent();
            }
        }

        [HttpDelete("broadcast/{operationId:guid}")]
        public async Task<IActionResult> DeleteTransactionState(
            OperationRequest request)
        {
            if (await _transactionService.DeleteTransactionIfExistsAsync(request.OperationId))
            {
                return Ok();
            }
            else
            {
                return NoContent();
            }
        }

        #region Not implemented endpoints
        
        [HttpPost("single/receive")]
        public ActionResult Build(
            [FromBody] BuildSingleReceiveTransactionRequest request)
                => StatusCode(StatusCodes.Status501NotImplemented);
        
        [HttpPost("many-inputs")]
        public ActionResult Build(
            [FromBody] BuildTransactionWithManyInputsRequest request)
                => StatusCode(StatusCodes.Status501NotImplemented);

        [HttpPost("many-outputs")]
        public ActionResult Build(
            [FromBody] BuildTransactionWithManyOutputsRequest request)
                => StatusCode(StatusCodes.Status501NotImplemented);
        
        [HttpGet("broadcast/many-inputs/{operationId:guid}")]
        public ActionResult GetManyInputsTransactionState(
            OperationRequest request)
                => StatusCode(StatusCodes.Status501NotImplemented);

        [HttpGet("broadcast/many-outputs/{operationId:guid}")]
        public ActionResult GetManyOutputsTransactionState(
            OperationRequest request)
                => StatusCode(StatusCodes.Status501NotImplemented);

        [HttpPut]
        public ActionResult Rebuild(
            [FromBody] RebuildTransactionRequest request)
                => StatusCode(StatusCodes.Status501NotImplemented);

        #endregion
    }
}

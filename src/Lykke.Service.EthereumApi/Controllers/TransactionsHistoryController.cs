using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using Lykke.Service.EthereumApi.Models;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumApi.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.EthereumApi.Controllers
{
    [PublicAPI, Route("/api/transactions/history")]
    public class TransactionsHistoryController : Controller
    {
        private readonly ITransactionHistoryService _transactionHistoryService;

        
        public TransactionsHistoryController(
            ITransactionHistoryService transactionHistoryService)
        {
            _transactionHistoryService = transactionHistoryService;
        }
        
        
        [HttpGet("to/{address}")]
        public async Task<ActionResult<IEnumerable<HistoricalTransactionContract>>> GetIncomingHistory(
            TransactionHistoryRequest request)
        {
            var transactions = await _transactionHistoryService.GetIncomingHistoryAsync
            (
                address: request.Address,
                take: request.Take,
                afterHash: request.AfterHash
            );
            
            return Ok(transactions.Select(MapTransactionReceipt));
        }

        [HttpGet("from/{address}")]
        public async Task<ActionResult<IEnumerable<HistoricalTransactionContract>>> GetOutgoingHistory(
            TransactionHistoryRequest request)
        {
            var transactions = await _transactionHistoryService.GetOutgoingHistoryAsync
            (
                address: request.Address,
                take: request.Take,
                afterHash: request.AfterHash
            );
            
            return Ok(transactions.Select(MapTransactionReceipt));
        }

        private static HistoricalTransactionContract MapTransactionReceipt(
            TransactionReceipt transactionReceipt)
        {
            return new HistoricalTransactionContract
            {
                Amount = transactionReceipt.Amount.ToString(),
                AssetId = Constants.AssetId,
                FromAddress = transactionReceipt.From,
                Hash = transactionReceipt.Hash,
                Timestamp = DateTimeOffset.FromUnixTimeSeconds((long) transactionReceipt.Timestamp).UtcDateTime,
                ToAddress = transactionReceipt.To
            };
        }
    }
}

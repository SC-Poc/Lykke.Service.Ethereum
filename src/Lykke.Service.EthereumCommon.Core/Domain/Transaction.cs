using System;
using System.Numerics;

namespace Lykke.Service.EthereumCommon.Core.Domain
{
    public class Transaction
    {
        private Transaction(
            BigInteger amount,
            DateTime builtOn,
            string data,
            string from,
            Guid operationId,
            TransactionState state,
            string to)
        {
            Amount = amount;
            BuiltOn = builtOn;
            From = from;
            OperationId = operationId;
            State = state;
            To = to;
            Data = data;
        }

        internal Transaction(
            BigInteger amount,
            BigInteger? blockNumber,
            DateTime? broadcastedOn,
            DateTime builtOn,
            DateTime? completedOn,
            string data,
            DateTime? deletedOn,
            string error,
            string from,
            Guid operationId,
            string signedData,
            TransactionState state,
            string to,
            string hash)
        {
            Amount = amount;
            BlockNumber = blockNumber;
            BroadcastedOn = broadcastedOn;
            BuiltOn = builtOn;
            CompletedOn = completedOn;
            Data = data;
            DeletedOn = deletedOn;
            Error = error;
            From = from;
            OperationId = operationId;
            SignedData = signedData;
            State = state;
            To = to;
            Hash = hash;
        }
        
        public static Transaction Build(
            Guid operationId,
            string from,
            string to,
            BigInteger amount,
            string data)
        {
            return new Transaction
            (
                amount: amount,
                builtOn: DateTime.UtcNow,
                data: data,
                from: from,
                operationId: operationId,
                state: TransactionState.Built,
                to: to
            );
        }
        
        
        
        public BigInteger Amount { get; }

        public BigInteger? BlockNumber { get; private set; }
        
        public DateTime? BroadcastedOn { get; private set; }
        
        public DateTime BuiltOn { get; }
        
        public DateTime? CompletedOn { get; private set; }

        public string Data { get; }
        
        public DateTime? DeletedOn { get; private set; }
        
        public string Error { get; private set; }
        
        public string From { get; }

        public string Hash { get; private set; }
        
        public Guid OperationId { get; }

        public string SignedData { get; private set; }

        public TransactionState State { get; private set; }

        public string To { get; }

        

        public void OnBroadcasted(
            string signedData,
            string hash)
        {
            if (State == TransactionState.Built)
            {
                BroadcastedOn = DateTime.UtcNow;
                SignedData = signedData;
                State = TransactionState.InProgress;
                Hash = hash;
            }
            else
            {
                throw new InvalidOperationException
                (
                    $"Transaction can not be broadcasted from current [{State.ToString()}] state."
                );
            }
        }

        public void OnSucceded(
            BigInteger blockNumber)
        {
            if (State == TransactionState.InProgress)
            {
                BlockNumber = blockNumber;
                CompletedOn = DateTime.UtcNow;
                State = TransactionState.Completed;
            }
            else
            {
                throw new InvalidOperationException
                (
                    $"Transaction can not succeed from current [{State.ToString()}] state."
                );
            }
        }

        public void OnFailed(
            BigInteger blockNumber,
            string error)
        {
            if (State == TransactionState.InProgress)
            {
                BlockNumber = blockNumber;
                CompletedOn = DateTime.UtcNow;
                Error = error;
                State = TransactionState.Failed;
            }
            else
            {
                throw new InvalidOperationException
                (
                    $"Transaction can not fail from current [{State.ToString()}] state."
                );
            }
        }
        
        public void OnDeleted()
        {
            if (State != TransactionState.Deleted)
            {
                DeletedOn = DateTime.UtcNow;
                State = TransactionState.Deleted;
            }
        }
    }
}

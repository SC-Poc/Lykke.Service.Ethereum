using System;
using System.Numerics;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.EthereumCommon.Core;

namespace Lykke.Service.EthereumCommon.AzureRepositories.Entities
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class TransactionEntity : AzureTableEntity
    {
        private BigInteger _amount;
        private DateTime _builtOn;
        private BigInteger _gasAmount;
        private BigInteger _gasPrice;
        private bool _includeFee;
        private TransactionState _state;
        private Guid _transactionId;
        
        
        public BigInteger Amount
        {
            get 
                => _amount;
            set
            {
                if (_amount != value)
                {
                    _amount = value;

                    MarkValueTypePropertyAsDirty(nameof(Amount));
                }
            }
        }

        public BigInteger? BlockNumber { get; set; }

        public DateTime? BroadcastedOn { get; set; }

        public DateTime BuiltOn
        {
            get 
                => _builtOn;
            set
            {
                if (_builtOn != value)
                {
                    _builtOn = value;

                    MarkValueTypePropertyAsDirty(nameof(BuiltOn));
                }
            }
        }

        public DateTime? CompletedOn { get; set; }

        public string Data { get; set; }

        public DateTime? DeletedOn { get; set; }

        public string Error { get; set; }

        public string From { get; set; }

        public BigInteger GasAmount
        {
            get 
                => _gasAmount;
            set
            {
                if (_gasAmount != value)
                {
                    _gasAmount = value;

                    MarkValueTypePropertyAsDirty(nameof(GasAmount));
                }
            }
        }

        public BigInteger GasPrice
        {
            get 
                => _gasPrice;
            set
            {
                if (_gasPrice != value)
                {
                    _gasPrice = value;

                    MarkValueTypePropertyAsDirty(nameof(GasPrice));
                }
            }
        }

        public string Hash { get; set; }

        public bool IncludeFee
        {
            get 
                => _includeFee;
            set
            {
                if (_includeFee != value)
                {
                    _includeFee = value;

                    MarkValueTypePropertyAsDirty(nameof(IncludeFee));
                }
            }
        }

        public string SignedData { get; set; }

        public TransactionState State
        {
            get 
                => _state;
            set
            {
                if (_state != value)
                {
                    _state = value;

                    MarkValueTypePropertyAsDirty(nameof(State));
                }
            }
        }

        public string To { get; set; }

        public Guid TransactionId
        {
            get 
                => _transactionId;
            set
            {
                if (_transactionId != value)
                {
                    _transactionId = value;

                    MarkValueTypePropertyAsDirty(nameof(TransactionId));
                }
            }
        }
    }
}

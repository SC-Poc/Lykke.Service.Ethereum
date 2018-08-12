using System.Runtime.Serialization;

namespace Lykke.Service.EthereumWorker.Services.Models
{
    public class TransactionTraceResponse
    {
        [DataMember(Name = "action")]
        public TransactionAction Action { get; set; }

        [DataMember(Name = "blockHash")]
        public string BlockHash { get; set; }

        [DataMember(Name = "blockNumber")]
        public ulong BlockNumber { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }

        [DataMember(Name = "result")]
        public TransactionResult Result { get; set; }

        [DataMember(Name = "subtraces")]
        public int Subtraces { get; set; }

        [DataMember(Name = "traceAddresses")]
        public int[] TraceAddresses { get; set; }

        [DataMember(Name = "transactionHash")]
        public string TransactionHash { get; set; }

        [DataMember(Name = "transactionPosition")]
        public int TransactionPosition { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }


        public class TransactionAction
        {
            [DataMember(Name = "callType")]
            public string CallType { get; set; }

            [DataMember(Name = "from")]
            public string From { get; set; }
        
            [DataMember(Name = "to")]
            public string To { get; set; }
        
            [DataMember(Name = "gas")]
            public string Gas { get; set; }
        
            [DataMember(Name = "value")]
            public string Value { get; set; }
        }

        public class TransactionResult
        {
            [DataMember(Name = "gasUsed")]
            public string GasUsed { get; set; }

            [DataMember(Name = "output")]
            public string Output { get; set; }

            [DataMember(Name = "address")]
            public string Address { get; set; }
        }
    }
}

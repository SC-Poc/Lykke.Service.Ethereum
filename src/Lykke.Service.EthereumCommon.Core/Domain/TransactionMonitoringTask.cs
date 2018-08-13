using System;
using MessagePack;

namespace Lykke.Service.EthereumCommon.Core.Domain
{
    [MessagePackObject]
    public class TransactionMonitoringTask
    {
        [Key(0)]
        public Guid TransactionId { get; set; }
    }
}

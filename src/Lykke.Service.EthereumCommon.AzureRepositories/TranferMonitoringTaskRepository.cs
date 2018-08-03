using AzureStorage.Queue;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;

namespace Lykke.Service.EthereumCommon.AzureRepositories
{
    [UsedImplicitly]
    public class TranferMonitoringTaskRepository : TaskRepositoryBase<TransactionMonitoringTask>, ITransactionMonitoringTaskRepository
    {
        internal TranferMonitoringTaskRepository(
            IQueueExt queue)
            : base(queue)
        {
            
        }
    }
}

using AzureStorage.Queue;
using Lykke.Service.EthereumCommon.AzureRepositories;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.Service.EthereumWorker.Core.Repositories;


namespace Lykke.Service.EthereumWorker.AzureRepositories
{
    public class BalanceObservationTaskRepository : TaskRepositoryBase<BalanceObservationTask>, IBalanceObservationTaskRepository
    {
        internal BalanceObservationTaskRepository(
            IQueueExt queue) 
            : base(queue)
        {
            
        }
    }
}

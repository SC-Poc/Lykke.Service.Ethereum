using System;
using AzureStorage.Queue;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Domain;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.SettingsReader;


namespace Lykke.Service.EthereumCommon.AzureRepositories
{
    [UsedImplicitly]
    public class TransactionMonitoringTaskRepository : TaskRepositoryBase<TransactionMonitoringTask>, ITransactionMonitoringTaskRepository
    {
        private TransactionMonitoringTaskRepository(
            IQueueExt queue)
            : base(queue)
        {
            
        }

        public static ITransactionMonitoringTaskRepository Create(
            IReloadingManager<string> connectionString)
        {
            var queue = AzureQueueExt.Create
            (
                connectionStringManager: connectionString,
                queueName: "transaction-monitoring-tasks",
                maxExecutionTimeout: TimeSpan.FromDays(7)
            );
            
            return new TransactionMonitoringTaskRepository(queue);
        }
    }
}

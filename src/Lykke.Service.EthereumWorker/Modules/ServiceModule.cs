using Autofac;
using Common;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumCommon.AzureRepositories;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.Service.EthereumWorker.AzureRepositories;
using Lykke.Service.EthereumWorker.Core.Repositories;
using Lykke.Service.EthereumWorker.Core.Services;
using Lykke.Service.EthereumWorker.QueueConsumers;
using Lykke.Service.EthereumWorker.Services;
using Lykke.Service.EthereumWorker.Settings;
using Lykke.SettingsReader;
using Nethereum.Parity;


namespace Lykke.Service.EthereumWorker.Modules
{
    [UsedImplicitly]
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }
        
        
        private WorkerSettings ServiceSettings
            => _appSettings.CurrentValue.WorkerService;
        

        protected override void Load(
            ContainerBuilder builder)
        {
            LoadQueueConsumers(builder);
            
            LoadRepositories(builder);
            
            LoadServices(builder);
        }

        private void LoadQueueConsumers(
            ContainerBuilder builder)
        {
            // BalanceObservationQueueConsumer

            builder
                .RegisterType<BalanceObservationQueueConsumer>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();
            
            builder
                .RegisterInstance(new BalanceObservationQueueConsumer.Settings
                {
                    MaxDegreeOfParallelism = ServiceSettings.BalanceObservationMaxDegreeOfParallelism
                })
                .AsSelf();
            
            // BlockchainIndexationQueueConsumer

            builder
                .RegisterType<BlockchainIndexationQueueConsumer>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();
            
            builder
                .RegisterInstance(new BlockchainIndexationQueueConsumer.Settings
                {
                    MaxDegreeOfParallelism = ServiceSettings.BlockchainIndexingMaxDegreeOfParallelism
                })
                .AsSelf();
            
            // TransactionMonitoringQueueConsumer

            builder
                .RegisterType<TransactionMonitoringQueueConsumer>()
                .As<IStartable>()
                .As<IStopable>()
                .SingleInstance();
            
            builder
                .RegisterInstance(new TransactionMonitoringQueueConsumer.Settings
                {
                    MaxDegreeOfParallelism = ServiceSettings.TransactionMonitoringMaxDegreeOfParallelism
                })
                .AsSelf();
        }
        
        private void LoadRepositories(
            ContainerBuilder builder)
        {
            var connectionString = _appSettings.ConnectionString(x => x.WorkerService.Db.DataConnString);
            
            // BalanceObservationTaskRepository
            
            builder
                .Register(x => BalanceObservationTaskRepository.Create
                (
                    connectionString: connectionString
                ))
                .As<IBalanceObservationTaskRepository>()
                .SingleInstance();
            
            // BlockchainIndexationStateRepository
            
            builder
                .Register(x => BlockchainIndexationStateRepository.Create
                (
                    connectionString: connectionString,
                    logFactory: x.Resolve<ILogFactory>()
                ))
                .As<IBlockchainIndexationStateRepository>()
                .SingleInstance();
            
            // BlockIndexationLockRepository
            
            builder
                .Register(x => BlockIndexationLockRepository.Create
                (
                    connectionString: connectionString,
                    logFactory: x.Resolve<ILogFactory>()
                ))
                .As<IBlockIndexationLockRepository>()
                .SingleInstance();
            
            // ObservableBalanceRepository
            
            builder
                .Register(x => ObservableBalanceRepository.Create
                (
                    connectionString: connectionString,
                    logFactory: x.Resolve<ILogFactory>()
                ))
                .As<IObservableBalanceRepository>()
                .SingleInstance();
            
            // TransactionMonitoringTaskRepository
            
            builder
                .Register(x => TransactionMonitoringTaskRepository.Create
                (
                    connectionString: connectionString
                ))
                .As<ITransactionMonitoringTaskRepository>()
                .SingleInstance();
            
            // TransactionReceiptRepository
            
            builder
                .Register(x => TransactionReceiptRepository.Create
                (
                    connectionString: connectionString,
                    logFactory: x.Resolve<ILogFactory>()
                ))
                .As<ITransactionReceiptRepository>()
                .SingleInstance();
            
            // TransactionRepository
            
            builder
                .Register(x => TransactionRepository.Create
                (
                    connectionString: connectionString,
                    logFactory: x.Resolve<ILogFactory>()
                ))
                .As<ITransactionRepository>()
                .SingleInstance();
            
        }
        
        private void LoadServices(
            ContainerBuilder builder)
        {
            // BalanceObservationService
            
            builder
                .RegisterType<BalanceObservationService>()
                .As<IBalanceObservationService>()
                .SingleInstance();
            
            // BlockchainIndexingService
            
            builder
                .RegisterType<BlockchainIndexingService>()
                .As<IBlockchainIndexingService>()
                .SingleInstance();
            
            builder
                .RegisterInstance(new BlockchainIndexingService.Settings
                {
                    BlockLockDuration = ServiceSettings.BlockLockDuration
                })
                .AsSelf();
            
            // BlockchainService
            
            builder
                .RegisterType<BlockchainService>()
                .As<IBlockchainService>()
                .SingleInstance();
            
            builder
                .RegisterInstance(new BlockchainService.Settings
                {
                    ConfirmationLevel = ServiceSettings.ConfirmationLevel,
                    ParityNodeUrl = ServiceSettings.ParityNodeUrl
                })
                .AsSelf();
            
            // TransactionMonitoringService
            
            builder
                .RegisterType<TransactionMonitoringService>()
                .As<ITransactionMonitoringService>()
                .SingleInstance();
        }
    }
}

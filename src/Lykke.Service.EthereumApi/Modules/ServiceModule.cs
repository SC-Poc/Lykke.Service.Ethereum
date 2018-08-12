using System.Numerics;
using Autofac;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.EthereumApi.Core.Services;
using Lykke.Service.EthereumApi.Services;
using Lykke.Service.EthereumApi.Settings;
using Lykke.Service.EthereumCommon.AzureRepositories;
using Lykke.Service.EthereumCommon.Core.Repositories;
using Lykke.SettingsReader;
using Nethereum.Parity;


namespace Lykke.Service.EthereumApi.Modules
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


        private ApiSettings ServiceSettings
            => _appSettings.CurrentValue.ApiService;
        
        
        protected override void Load(
            ContainerBuilder builder)
        {
            LoadRepositories(builder);
            
            LoadServices(builder);
            
            LoadUtils(builder);
        }

        private void LoadRepositories(
            ContainerBuilder builder)
        {
            var connectionString = _appSettings.ConnectionString(x => x.ApiService.Db.DataConnString);
            
            // ObservableBalanceRepository

            builder
                .Register(x => ObservableBalanceRepository.Create
                (
                    connectionString: connectionString,
                    logFactory: x.Resolve<ILogFactory>()
                ))
                .As<IObservableBalanceRepository>()
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
            
            // TransactionMonitoringTaskRepository
            
            builder
                .Register(x => TransactionMonitoringTaskRepository.Create
                (
                    connectionString
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
        }

        private void LoadServices(
            ContainerBuilder builder)
        {
            // AddressService
            
            builder
                .RegisterType<AddressService>()
                .As<IAddressService>()
                .SingleInstance();
            
            // BalanceService
            
            builder
                .RegisterType<BalanceService>()
                .As<IBalanceService>()
                .SingleInstance();

            // BlockchainService
            
            builder
                .RegisterType<BlockchainService>()
                .As<IBlockchainService>()
                .SingleInstance();

            builder
                .RegisterInstance(new BlockchainService.Settings
                {
                    MaxGasPriceManager = _appSettings.Nested(x => x.ApiService.MaximalGasPrice),
                    MinGasPriceManager = _appSettings.Nested(x => x.ApiService.MinimalGasPrice)
                })
                .AsSelf();
            
                
            // TransactionHistoryService
            
            builder
                .RegisterType<TransactionHistoryService>()
                .As<ITransactionHistoryService>()
                .SingleInstance();
            
            // TransactionService
            
            builder
                .RegisterType<TransactionService>()
                .As<ITransactionService>()
                .SingleInstance();

            builder
                .RegisterInstance(new TransactionService.Settings
                {
                    MinimalTransactionAmount = BigInteger.Parse(ServiceSettings.MinimalTransactionAmount)
                })
                .AsSelf();
        }

        private void LoadUtils(
            ContainerBuilder builder)
        {
            // Web3

            builder
                .RegisterInstance(new Web3Parity
                (
                    url: ServiceSettings.ParityNodeUrl
                ))
                .AsSelf();
        }
    }
}

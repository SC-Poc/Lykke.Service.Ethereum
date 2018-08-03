using Autofac;
using Lykke.Service.EthereumWorker.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.EthereumWorker.Modules
{    
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            
        }
    }
}

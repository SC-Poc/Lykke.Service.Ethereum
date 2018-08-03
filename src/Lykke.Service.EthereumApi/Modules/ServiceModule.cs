using Autofac;
using Lykke.Service.EthereumApi.Settings;
using Lykke.SettingsReader;

namespace Lykke.Service.EthereumApi.Modules
{    
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _appSettings;

        public ServiceModule(IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            
        }
    }
}

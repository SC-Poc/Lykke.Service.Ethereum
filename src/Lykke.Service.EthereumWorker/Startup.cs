using System;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.EthereumCommon;
using Lykke.Service.EthereumWorker.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


namespace Lykke.Service.EthereumWorker
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Startup
    {
        public void Configure(
            IApplicationBuilder app)
        {
            app.UseLykkeConfiguration();
        }

        public IServiceProvider ConfigureServices(
            IServiceCollection services)
        {
            return services.BuildEthereumServiceProvider<AppSettings>
            (
                serviceName: "Worker",
                enableLogging: true,
                logsConnString: settings => settings.WorkerService.Db.LogsConnString
            );
        }
    }
}

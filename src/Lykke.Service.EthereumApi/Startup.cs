using System;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.EthereumApi.Settings;
using Lykke.Service.EthereumCommon;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;


namespace Lykke.Service.EthereumApi
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
            return services.BuildServiceProvider<AppSettings>
            (
                serviceName: "Api",
                enableLogging: true,
                logsConnString: settings => settings.ApiService.Db.LogsConnString
            );
        }
    }
}

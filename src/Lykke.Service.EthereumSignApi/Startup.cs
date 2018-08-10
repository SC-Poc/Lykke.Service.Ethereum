using System;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.EthereumCommon;
using Lykke.Service.EthereumCommon.Core;
using Lykke.Service.EthereumSignApi.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.EthereumSignApi
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {   
            app.UseLykkeConfiguration();
        }

        public IServiceProvider ConfigureServices(
            IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>
            (
                serviceName: "ApiLog",
                
                #if ENABLE_SENSITIVE_LOGGING
                
                enableLogging: true,
                
                #else

                enableLogging: false,

                #endif
                
                logsConnString: settings => settings.SignApiService.Db.LogsConnString
            );
        }
    }
}

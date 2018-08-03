using System;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.EthereumApi.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.EthereumApi
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Startup
    {
        public IServiceProvider ConfigureServices(
            IServiceCollection services)
        {                                   
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Logs = logs =>
                {
                    logs.AzureTableName 
                        = "EthereumApiLog";
                    
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.EthereumApiService.Db.LogsConnString;
                };
                options.SwaggerOptions = new LykkeSwaggerOptions
                {
                    ApiTitle = "Ethereum Api"
                };
            });
        }

        public void Configure(
            IApplicationBuilder app)
        {
            app.UseLykkeConfiguration();
        }
    }
}

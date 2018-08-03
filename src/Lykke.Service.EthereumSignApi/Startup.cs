using System;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.EthereumSignApi.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.EthereumSignApi
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Startup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {        
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Logs = logs =>
                {
                    logs.AzureTableName 
                        = "EthereumSignApiLog";
                    
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.EthereumSignApiService.Db.LogsConnString;
                };
                options.SwaggerOptions = new LykkeSwaggerOptions
                {
                    ApiTitle = "Ethereum Sign Api"
                };
            });
        }

        public void Configure(IApplicationBuilder app)
        {   
            app
                .UseLykkeConfiguration();
        }
    }
}

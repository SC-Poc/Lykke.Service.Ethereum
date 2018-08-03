using System;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.EthereumApi.Settings;
using Lykke.Service.EthereumCommon.Core;
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
                        = $"{Constants.BlockchainId}ApiLog";
                    
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.ApiService.Db.LogsConnString;
                };
                options.SwaggerOptions = new LykkeSwaggerOptions
                {
                    ApiTitle = $"{Constants.BlockchainName} Api"
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

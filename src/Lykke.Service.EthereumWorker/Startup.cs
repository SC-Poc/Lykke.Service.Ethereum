using System;
using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.EthereumCommon.Core;
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
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.Logs = logs =>
                {
                    logs.AzureTableName 
                        = $"{Constants.BlockchainId}WorkerLog";
                    
                    logs.AzureTableConnectionStringResolver =
                        settings => settings.WorkerService.Db.LogsConnString;
                };
                options.SwaggerOptions = new LykkeSwaggerOptions
                {
                    ApiTitle = $"{Constants.BlockchainName} Worker Api"
                };
            });
        }
    }
}

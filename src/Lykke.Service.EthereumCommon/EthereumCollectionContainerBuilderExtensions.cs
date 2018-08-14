// ReSharper disable RedundantUsingDirective

using System;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeSlack;
using Lykke.Sdk;
using Lykke.Sdk.Settings;
using Lykke.Service.EthereumCommon.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using LogLevel = Microsoft.Extensions.Logging.LogLevel;

// ReSharper restore RedundantUsingDirective


namespace Lykke.Service.EthereumCommon
{
    public static class EthereumCollectionContainerBuilderExtensions
    {
        public static IServiceProvider BuildEthereumServiceProvider<T>(
            this IServiceCollection services,
            string serviceName,
            bool enableLogging,
            Func<T, string> logsConnString)

            where T : BaseAppSettings
        {
            return services.BuildServiceProvider<T>(options =>
            {
                options.Logs = logs =>
                {
                    if (enableLogging)
                    {
                        logs.AzureTableName
                            = $"{Constants.BlockchainId}{serviceName}Log";

                        logs.AzureTableConnectionStringResolver
                            = logsConnString;

                        #if !DEBUG

                        logs.Extended = extendedLogs =>
                        {
                            extendedLogs.AddAdditionalSlackChannel("BlockChainIntegration", channelOptions =>
                            {
                                channelOptions.MinLogLevel = LogLevel.Information;
                            });
    
                            extendedLogs.AddAdditionalSlackChannel("BlockChainIntegrationImportantMessages", channelOptions =>
                            {
                                channelOptions.MinLogLevel = LogLevel.Warning;
                                channelOptions.SpamGuard.DisableGuarding();
                            });
                        };
    
                        #else

                        logs.Extended = extendedLogs =>
                        {
                            extendedLogs.SetMinimumLevel(LogLevel.Trace);
                        };

                        #endif
                    }
                    else
                    {
                        logs.UseEmptyLogging();
                    }
                };
                
                options.SwaggerOptions = new LykkeSwaggerOptions
                {
                    ApiTitle = $"{Constants.BlockchainName} {serviceName}"
                };
            });
        }
    }
}

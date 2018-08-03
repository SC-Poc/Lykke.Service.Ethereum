using System;
using System.Threading.Tasks;
using Lykke.Service.EthereumCommon.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.PlatformAbstractions;

namespace Lykke.Service.EthereumCommon
{
    public abstract class ProgramBase
    {
        private static ApplicationEnvironment Application
            => PlatformServices.Default.Application;
        
        public static string EnvInfo 
            => Environment.GetEnvironmentVariable("ENV_INFO");
        
        
        protected static async Task RunAsync(Func<IWebHost> hostFactory)
        {
            Console.WriteLine($"{Application.ApplicationName} version {Application.ApplicationVersion}");
            Console.WriteLine(Constants.BuildConfigurationMessage);

            try
            {
                var host = hostFactory();
                
                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fatal error:");
                Console.WriteLine(ex);

                await DelayTerminationAsync();
            }

            Console.WriteLine("Terminated");
        }
        
        private static async Task DelayTerminationAsync()
        {
            var delay = TimeSpan.FromMinutes(1);

            Console.WriteLine();
            Console.WriteLine($"Process will be terminated in {delay}. Press any key to terminate immediately.");

            await Task.WhenAny
            (
                Task.Delay(delay),
                Task.Run(() =>
                {
                    Console.ReadKey(true);
                })
            );
        }
    }
}

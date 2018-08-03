using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon;
using Microsoft.AspNetCore.Hosting;

namespace Lykke.Service.EthereumSignApi
{
    [UsedImplicitly]
    internal sealed class Program : ProgramBase
    {
        public static async Task Main()
        {
            await RunAsync(() => new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:5000")
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build());
        }
    }
}

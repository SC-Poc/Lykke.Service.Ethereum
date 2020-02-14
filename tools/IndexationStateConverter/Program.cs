using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.EthereumWorker.AzureRepositories;
using Lykke.Service.EthereumWorker.Core.Domain;
using Lykke.SettingsReader.ReloadingManager;
using Newtonsoft.Json;

namespace IndexationStateConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Usage: dotnet IndexationStateEditor.dll read|write <bil-ethereum-worker-azure-storage-connection-string> ");

            if (args.Length != 2)
            {
                Console.WriteLine("Invalid parameters count");
                return;
            }

            var connectionString = ConstantReloadingManager.From(args[1]);
            var repository = BlockchainIndexationStateRepository.Create(connectionString);

            if (args[0] == "read")
            {
                Read(repository).Wait();
            }
            else if (args[0] == "write")
            {
                Write(repository).Wait();
            }
            else
            {
                Console.WriteLine($"Unknown command: {args[0]}");
            }
        }

        private static async Task Read(BlockchainIndexationStateRepository repository)
        {
            Console.WriteLine("Reading the state from BLOB...");

            var state = await repository.GetOrCreateAsync();

            Console.WriteLine("Serializing the state to json...");

            var json = JsonConvert.SerializeObject(state.AsEnumerable(), Formatting.Indented);

            Console.WriteLine("Serializing the state json to the 'state.json' file...");

            await File.WriteAllTextAsync("state.json", json);
        }

        private static async Task Write(BlockchainIndexationStateRepository repository)
        {
            Console.WriteLine("Reading the state from the 'state.json' file...");

            var json = await File.ReadAllTextAsync("state.json");

            Console.WriteLine("Deserializing the state from json...");

            var intervals = JsonConvert.DeserializeObject<IEnumerable<BlocksIntervalIndexationState>>(json);
            var state = BlockchainIndexationState.Restore(intervals);

            var x = state.GetNonIndexedBlockNumbers();
            
            Console.WriteLine("Saving the state to BLOB...");

            await repository.UpdateAsync(state);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Multiformats.Hash;
using Multiformats.Hash.Algorithms;

namespace Lykke.Service.EthereumCommon.Core.Crypto
{
    public static class Keccak256
    {
        public static byte[] Sum(params byte[][] data)
        {
            var multihash = Multihash.Sum<KECCAK_256>
            (
                data: ConcatMany(data)
            );

            return multihash.Digest;
        }
        
        public static async Task<byte[]> SumAsync(params byte[][] data)
        {
            var multihash = await Multihash.SumAsync<KECCAK_256>
            (
                data: ConcatMany(data)
            );

            return multihash.Digest;
        }
        
        private static byte[] ConcatMany(IEnumerable<byte[]> data)
        {
            return data
                .SelectMany(x => x)
                .ToArray();
        }
    }
}

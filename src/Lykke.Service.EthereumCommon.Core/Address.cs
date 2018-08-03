using System;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Crypto;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Lykke.Service.EthereumCommon.Core
{
    [PublicAPI]
    public static class Address
    {
        private static readonly Regex AddressStringExpression
            = new Regex(@"^0x[0-9a-fA-F]{40}$", RegexOptions.Compiled);
        
        
        public static bool ValidateFormat(
            string addressString)
        {
            return AddressStringExpression.IsMatch(addressString);
        }

        public static bool ValidateFormatAndChecksum(
            string addressString)
        {
            return ValidateFormat(addressString)
                && ValidateChecksum(addressString);
        }

        private static bool ValidateChecksum(
            string addressString)
        {
            var addressBytes = Encoding.UTF8.GetBytes(addressString.ToLowerInvariant());
            var caseMapBytes = Keccak256.Sum(addressBytes);
        
            for (var i = 0; i < 40; i++)
            {
                var addressChar = addressString[i];
        
                if (!char.IsLetter(addressChar))
                {
                    continue;
                }
        
                var leftShift = i % 2 == 0 ? 7 : 3;
                var shouldBeUpper = (caseMapBytes[i / 2] & (1 << leftShift)) != 0;
                var shouldBeLower = !shouldBeUpper;
        
                if (shouldBeUpper && char.IsLower(addressChar) ||
                    shouldBeLower && char.IsUpper(addressChar))
                {
                    return false;
                }
            }
        
            return true;
        }
        
        private static void ValidateFormatAndThrowIfInvalid(
            string addressString)
        {
            if (!ValidateFormat(addressString))
            {
                throw new FormatException("Specified address string is not in valid format.");
            }
        }
    }
}

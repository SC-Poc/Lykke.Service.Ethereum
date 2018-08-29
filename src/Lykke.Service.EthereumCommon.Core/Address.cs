using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Crypto;


namespace Lykke.Service.EthereumCommon.Core
{
    [PublicAPI]
    public static class Address
    {
        private static readonly Regex AddressStringExpression
            = new Regex(@"^0x[0-9a-fA-F]{40}$", RegexOptions.Compiled);


        public static string AddChecksum(
            string addressString)
        {
            ValidateFormatAndThrowIfInvalid(addressString);
            
            addressString = addressString.Remove(0, 2).ToLowerInvariant();

            var addressBytes = Encoding.UTF8.GetBytes(addressString);
            var caseMapBytes = Keccak256.Sum(addressBytes);
                
            var addressBuilder = new StringBuilder("0x");
                
            for (var i = 0; i < 40; i++)
            {
                var addressChar = addressString[i];
                
                if (char.IsLetter(addressChar))
                {
                    var leftShift = i % 2 == 0 ? 7 : 3;
                    var shouldBeUpper = (caseMapBytes[i / 2] & (1 << leftShift)) != 0;

                    if (shouldBeUpper)
                    {
                        addressChar = char.ToUpper(addressChar);
                    }
                }
                    
                addressBuilder.Append(addressChar);
            }

            return addressBuilder.ToString();
        }
        
        
        public static bool ValidateFormatAndChecksum(
            string addressString,
            bool allowAllLowerCase,
            bool allowAllUpperCase)
        {
            if (!ValidateFormat(addressString))
            {
                return false;
            }

            if (allowAllLowerCase && addressString.Skip(2).Where(char.IsLetter).All(char.IsLower))
            {
                return true;
            }
            
            if (allowAllUpperCase && addressString.Skip(2).Where(char.IsLetter).All(char.IsUpper))
            {
                return true;
            }

            return ValidateChecksum(addressString);
        }

        private static bool ValidateChecksum(
            string addressString)
        {
            addressString = addressString.Remove(0, 2);
            
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
        
        private static bool ValidateFormat(
            string addressString)
        {
            return AddressStringExpression.IsMatch(addressString);
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

using System;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Lykke.Service.EthereumCommon.Core.Crypto;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Lykke.Service.EthereumCommon.Core
{
    [PublicAPI]
    public class Address
    {
        private static readonly Regex AddressStringExpression
            = new Regex(@"^(?:0x){0,1}[0-9a-fA-F]{40}$", RegexOptions.Compiled);
        
        private readonly byte[] _addressBytes;
        
        
        public Address(
            byte[] addressBytes)
        {
            if (addressBytes.Length != 20)
            {
                throw new ArgumentException
                (
                    "Address should contain 20 bytes",
                    nameof(addressBytes)
                );
            }

            _addressBytes = addressBytes;
        }
        
        public static Address Parse(
            string addressString)
        {
            ValidateFormatAndThrowIfInvalid(addressString);

            if (addressString.StartsWith("0x"))
            {
                addressString = addressString.Remove(0, 2);
            }
            
            return new Address
            (
                addressBytes: addressString.HexToByteArray()
            );
        }
        
        public static bool TryParse(
            string addressString,
            out Address address)
        {
            try
            {
                address = Parse(addressString);

                return true;
            }
            catch (Exception e)
            {
                address = null;

                return false;
            }
        }

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

        /// <inheritdoc cref="object.ToString"/>
        public override string ToString()
        {
            return ToString("0xLC");
        }

        /// <summary>
        ///    Formats the value of the current instance using the specified format.
        /// </summary>
        /// <param name="format">
        ///     0xLC - lower-case address with 0x prefix
        ///     0xUC - upper-case address with 0x prefix
        ///     0xCS - checksum address with 0x prefix
        ///     LC - lower-case address with no prefix
        ///     UC - upper-case address with no prefix
        ///     CS - checksum address with no prefix
        /// </param>
        public string ToString(string format)
        {
            return ToString(format, null);
        }

        /// <inheritdoc cref="IFormattable.ToString(string,System.IFormatProvider)"/>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            var addressString = _addressBytes.ToHex();

            void AddChecksumToAddressString()
            {
                var addressBytes = Encoding.UTF8.GetBytes(addressString);
                var caseMapBytes = Keccak256.Sum(addressBytes);
                
                var addressBuilder = new StringBuilder();
                
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

                addressString = addressBuilder.ToString();
            }

            void AddHexPrefixToAddressString()
            {
                addressString = $"0x{addressString}";
            }
            
            void LowercaseAddressString()
            {
                addressString = addressString.ToLowerInvariant();
            }
            
            void UppercaseAddressString()
            {
                addressString = addressString.ToUpperInvariant();
            }
            
            switch (format)
            {
                case "0xLC":
                    LowercaseAddressString();
                    AddHexPrefixToAddressString();
                    break;
                case "0xUC":
                    UppercaseAddressString();
                    AddHexPrefixToAddressString();
                    break;
                case "0xCS":
                    AddChecksumToAddressString();
                    AddHexPrefixToAddressString();
                    break;
                case "LC":
                    LowercaseAddressString();
                    break;
                case "UC":
                    UppercaseAddressString();
                    break;
                case "CS":
                    AddChecksumToAddressString();
                    break;
                default:
                    throw new FormatException($"The '{format}' format string is not supported.");
            }

            return addressString;
        }

        public static implicit operator byte[](
            Address address)
        {
            return address._addressBytes;
        }

        public static explicit operator Address(
            byte[] addressBytes)
        {
            return new Address(addressBytes);
        }
    }
}

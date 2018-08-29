using System;
using System.Numerics;
using System.Text.RegularExpressions;
using FluentValidation;
using Lykke.Service.EthereumCommon.Core;

namespace Lykke.Service.EthereumApi.Validation
{
    public static class Rules
    {
        private static readonly Regex HexStringExpression
            = new Regex(@"^0x[0-9a-fA-F]+$", RegexOptions.Compiled);
        
        
        public static void AmountMustBeValid<T>(
            this IRuleBuilderInitial<T, string> ruleBuilder)
        {
            ruleBuilder
                .Must(amount => BigInteger.TryParse(amount, out var amountParsed) && amountParsed > 0)
                .WithMessage(x => "Amount must be greater than zero.");
        }
        
        public static void TransactionIdMustBeNonEmptyGuid<T>(
            this IRuleBuilderInitial<T, Guid> ruleBuilder)
        {
            ruleBuilder
                .Must(transactionId => transactionId != Guid.Empty)
                .WithMessage(x => "Specified transaction id is empty.");
        }

        public static void AddressMustBeValid<T>(
            this IRuleBuilderInitial<T, string> ruleBuilder)
        {
            ruleBuilder
                .Must(x => Address.ValidateFormatAndChecksum(x, true, true))
                .WithMessage(x => $"Specified address is not valid.");
        }

        public static void AssetMustBeSupported<T>(
            this IRuleBuilderInitial<T, string> ruleBuilder)
        {
            ruleBuilder
                .Must(assetId => assetId == Constants.AssetId)
                .WithMessage(x => $"Specified asset is not supported.");
        }

        public static void MustBeHexString<T>(
            this IRuleBuilderInitial<T, string> ruleBuilder)
        {
            ruleBuilder
                .Must(@string => HexStringExpression.IsMatch(@string))
                .WithMessage(x => "Must be a hex string.");
        }
    }
}

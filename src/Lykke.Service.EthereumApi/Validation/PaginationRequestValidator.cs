using System;
using Common;
using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.EthereumApi.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;

namespace Lykke.Service.EthereumApi.Validation
{
    [UsedImplicitly]
    public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
    {
        public PaginationRequestValidator()
        {
            RuleFor(x => x.Take)
                .Must(take => take > 0)
                .WithMessage(x => "Take parameter must be greater than zero.");

            RuleFor(x => x.Continuation)
                .Must(ValidateContinuationToken)
                .WithMessage(x => "Continuation token is not valid.");
        }

        private static bool ValidateContinuationToken(string token)
        {
            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    var decodedToken = token.HexToUTF8String();

                    JsonConvert.DeserializeObject<TableContinuationToken>(decodedToken);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

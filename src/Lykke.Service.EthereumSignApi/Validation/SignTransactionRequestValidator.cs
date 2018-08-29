using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Service.EthereumSignApi.Validation
{
    [UsedImplicitly]
    public class SignTransactionRequestValidator : AbstractValidator<SignTransactionRequest>
    {
        public SignTransactionRequestValidator()
        {
            RuleFor(x => x.TransactionContext)
                .Must((request, ctx) => Regex.IsMatch(request.PrivateKeys.Single(), @"^0x[0-9a-f]+$"))
                .WithMessage(x => "Request contains transaction context, that is not properly formatted.");
            
            RuleFor(x => x.PrivateKeys)
                .Must((request, ctx) => request.PrivateKeys?.Count == 1)
                .WithMessage(x => "Request should contain single private key.");

            RuleFor(x => x.PrivateKeys)
                .Must((request, ctx) => Regex.IsMatch(request.PrivateKeys.Single(), @"^0x[0-9a-f]{64}$"))
                .WithMessage(x => "Request contains private key, that is not properly formatted.");
        }
    }
}

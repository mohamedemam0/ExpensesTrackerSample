using ExpensesTracker.Shared.DTOs;
using FluentValidation;
using System.Transactions;

namespace ExpensesTracker.Shared.Validators
{
    public class TransactionDtoValidator : AbstractValidator<TransactionDto>
    {
        public TransactionDtoValidator()
        {
            RuleFor(x => x.Amount).GreaterThanOrEqualTo(0).WithMessage("Amount cannot be negative.");
            RuleFor(x => x.WalletId).NotEmpty().WithMessage("Wallet is required.");
            RuleFor(x => x.Category).NotEmpty().WithMessage("Category is required.");
            RuleFor(x => x.DateTime).LessThanOrEqualTo(DateTime.UtcNow)
                .WithMessage("Date cannot be in the future.").When(x => x.DateTime != null);

        }

    }
}

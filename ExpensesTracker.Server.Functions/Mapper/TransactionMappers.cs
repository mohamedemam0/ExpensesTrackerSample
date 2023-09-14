using ExpensesTracker.Server.Data.Models;
using ExpensesTracker.Shared.DTOs;

namespace AKExpensesTracker.Server.Functions.Mapper
{
    public static class TransactionMappers
	{

		public static TransactionDto ToTransactionDto(this Transaction transaction)
		{
			return new()
			{
				Id = transaction.Id,
				Amount = transaction.Amount,
				Category = transaction.Category,
				DateTime = transaction.CreationDate,
				Description = transaction.Description,
				IsIncome = transaction.IsIncome,
				WalletId = transaction.WalletId,
				Tags = transaction.Tags,
				Attachments = transaction.Attachments,
			};
		}

	}
}

namespace ExpensesTracker.Server.Data.Interfaces
{
    public interface ITransactionRepo
    {
        Task DeleteAsync(Transaction transaction);
        Task CreateAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task<Transaction> GetByIdAsync(string id, string userId, int year);
        Task<IEnumerable<Transaction>> ListByUserIdAsync(string userId, int year, IEnumerable<string> walletIds, DateTime? minDate = null, DateTime? maxDate = null);
    }
}
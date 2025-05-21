using Groups_API.Models.Domain;
namespace Groups_API.Repositories.Interface
{
    public interface ITransactionRepository
    {
        Task<Transaction> CreateTransactionAsync(Transaction transaction);
        Task<List<Transaction>> GetTransactionsForGroupAsync(int groupId);
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task<bool> DeleteTransactionAsync(int id);
        Task UpdateDebtsIncrementally(int groupId, Transaction newTransaction);
    }

}

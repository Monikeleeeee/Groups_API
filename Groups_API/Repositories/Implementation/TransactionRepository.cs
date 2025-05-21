using Groups_API.Data;
using Groups_API.Models.Domain;
using Groups_API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Groups_API.Repositories.Implementation
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task UpdateDebtsIncrementally(int groupId, Transaction newTransaction)
        {
            var existingDebts = await _context.Debts
                .Where(d => d.GroupId == groupId)
                .ToListAsync();

            var debtsMap = existingDebts.ToDictionary(
                d => (d.DebtorId, d.CreditorId),
                d => d
            );

            var payerId = newTransaction.PayerId;

            foreach (var split in newTransaction.Splits)
            {
                var debtorId = split.MemberId;
                if (debtorId == payerId) continue;

                var amountOwed = split.Amount;

                var forwardKey = (debtorId, payerId);
                var reverseKey = (payerId, debtorId);

                if (debtsMap.TryGetValue(reverseKey, out var reverseDebt))
                {
                    if (reverseDebt.Amount > amountOwed)
                    {
                        reverseDebt.Amount -= amountOwed;
                        _context.Debts.Update(reverseDebt);
                    }
                    else if (reverseDebt.Amount < amountOwed)
                    {
                        _context.Debts.Remove(reverseDebt);
                        debtsMap.Remove(reverseKey);

                        var remaining = amountOwed - reverseDebt.Amount;
                        var newDebt = new Debt
                        {
                            GroupId = groupId,
                            DebtorId = debtorId,
                            CreditorId = payerId,
                            Amount = remaining
                        };
                        _context.Debts.Add(newDebt);
                        debtsMap[forwardKey] = newDebt;
                    }
                    else
                    {
                        _context.Debts.Remove(reverseDebt);
                        debtsMap.Remove(reverseKey);
                    }
                }
                else if (debtsMap.TryGetValue(forwardKey, out var existingDebt))
                {
                    existingDebt.Amount += amountOwed;
                    _context.Debts.Update(existingDebt);
                }
                else
                {
                    var newDebt = new Debt
                    {
                        GroupId = groupId,
                        DebtorId = debtorId,
                        CreditorId = payerId,
                        Amount = amountOwed
                    };
                    _context.Debts.Add(newDebt);
                    debtsMap[forwardKey] = newDebt;
                }
            }

            await _context.SaveChangesAsync();
        }




        public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<List<Transaction>> GetTransactionsForGroupAsync(int groupId)
        {
            return await _context.Transactions
                .Where(t => t.GroupId == groupId)
                .Include(t => t.Splits)
                .Include(t => t.Payer)
                .ToListAsync();
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions
                .Include(t => t.Splits)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}

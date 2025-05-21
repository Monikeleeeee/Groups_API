using Groups_API.Data;
using Groups_API.Models;
using Groups_API.Models.Domain;
using Groups_API.Models.DTO;
using Groups_API.Models.Enums;
using Groups_API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Groups_API.Repositories.Implementation
{
    public class GroupMembershipRepository : IGroupMembershipRepository
    {
        private readonly ApplicationDbContext _context;

        public GroupMembershipRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DebtDTO>> GetGroupDebtMatrix(int groupId)
        {
            return await _context.Debts
                .Where(d => d.GroupId == groupId && d.Amount > 0)
                .Select(d => new DebtDTO
                {
                    FromMemberId = d.DebtorId,
                    FromMemberName = d.Debtor.Name,
                    ToMemberId = d.CreditorId,
                    ToMemberName = d.Creditor.Name,
                    Amount = d.Amount
                })
                .ToListAsync();
        }
        public async Task<bool> Exists(int groupId)
        {
            return await _context.Groups.AnyAsync(g => g.Id == groupId);
        }

        public async Task<bool> SettleDebtBetweenMembers(int groupId, int memberAId, int memberBId)
        {
            var debtAB = await _context.Debts
                .FirstOrDefaultAsync(d => d.GroupId == groupId && d.DebtorId == memberAId && d.CreditorId == memberBId);

            var debtBA = await _context.Debts
                .FirstOrDefaultAsync(d => d.GroupId == groupId && d.DebtorId == memberBId && d.CreditorId == memberAId);

            if (debtAB == null && debtBA == null)
                return false; 

            if (debtAB != null)
                _context.Debts.Remove(debtAB);

            if (debtBA != null)
                _context.Debts.Remove(debtBA);

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> SettleDebtAsync(int groupId, int fromMemberId, int toMemberId)
        {
            var debt = await _context.Debts
                .FirstOrDefaultAsync(d =>
                    d.GroupId == groupId &&
                    d.DebtorId == fromMemberId &&
                    d.CreditorId == toMemberId);

            if (debt == null || debt.Amount <= 0)
                return false;

            var settlementTransaction = new Transaction
            {
                GroupId = groupId,
                PayerId = fromMemberId,
                TotalAmount = debt.Amount,
                Date = DateTime.UtcNow,
                SplitType = SplitType.Dynamic, 
                Splits = new List<TransactionSplit>
        {
            new TransactionSplit
            {
                MemberId = toMemberId,
                Amount = debt.Amount
            }
        }
            };

            _context.Transactions.Add(settlementTransaction);
            _context.Debts.Remove(debt);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<IEnumerable<TransactionDTO>> GetGroupTransactionsAsync(int groupId)
        {
            return await _context.Transactions
                .Where(t => t.GroupId == groupId)
                .Include(t => t.Payer)
                .OrderByDescending(t => t.Date)
                .Select(t => new TransactionDTO
                {
                    Id = t.Id,
                    PayerName = t.Payer.Name,
                    TotalAmount = t.TotalAmount,
                    Date = t.Date,
                    SplitType = t.SplitType.ToString()
                })
                .ToListAsync();
        }

    }
}

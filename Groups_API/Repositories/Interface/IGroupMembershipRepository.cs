using Groups_API.Models;
using Groups_API.Models.DTO;

namespace Groups_API.Repositories.Interface
{
    public interface IGroupMembershipRepository
    {
        Task<IEnumerable<DebtDTO>> GetGroupDebtMatrix(int groupId);
        Task<bool> Exists(int groupId);
        Task<bool> SettleDebtBetweenMembers(int groupId, int memberAId, int memberBId);
        Task<bool> SettleDebtAsync(int groupId, int fromMemberId, int toMemberId);
        Task<IEnumerable<TransactionDTO>> GetGroupTransactionsAsync(int groupId);

    }
}

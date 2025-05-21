using Groups_API.Data;
using Groups_API.Models.Domain;
using Groups_API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Groups_API.Repositories.Implementation
{
    public class MemberRepository : IMemberRepository
    {
        private readonly ApplicationDbContext _context;
        public MemberRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddMemberToGroup(Member member, Group group)
        {
            var membership = new GroupMembership
            {
                Member = member,
                Group = group,
                Balance = 0
            };

            _context.GroupMemberships.Add(membership);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> RemoveMemberFromGroup(int groupId, int memberId)
        {
            var membership = await _context.GroupMemberships
                .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.MemberId == memberId);

            if (membership == null)
                return false;

            _context.GroupMemberships.Remove(membership);
            await _context.SaveChangesAsync();
            return true;

        }
        public async Task<bool> HasOutstandingDebts(int groupId, int memberId)
        {
            return await _context.Debts.AnyAsync(d =>
                d.GroupId == groupId &&
                (d.DebtorId == memberId || d.CreditorId == memberId) &&
                d.Amount > 0);
        }
    }
}

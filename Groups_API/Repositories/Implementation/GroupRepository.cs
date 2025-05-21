using Groups_API.Data;
using Groups_API.Models.Domain;
using Groups_API.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Groups_API.Repositories.Implementation
{
    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationDbContext _context;

        public GroupRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Group>> GetAllGroups()
        {
            return await _context.Groups
                .Include(g => g.GroupMemberships) 
                    .ThenInclude(gm => gm.Member)
                .ToListAsync();
        }

        public async Task AddGroup(Group group)
        {
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(int groupId)
        {
            return await _context.Groups.AnyAsync(g => g.Id == groupId);
        }
        public async Task<Group> GetGroupById(int id)
        {
            return await _context.Groups
                .Include(g => g.GroupMemberships)
                    .ThenInclude(gm => gm.Member)
                .FirstOrDefaultAsync(g => g.Id == id);
        }
        public async Task<List<Group>> GetAllGroupsWithMembershipsAndDebts()
        {
            return await _context.Groups
                .Include(g => g.GroupMemberships)
                .Include(g => g.Debts)
                .ToListAsync();
        }


    }
}

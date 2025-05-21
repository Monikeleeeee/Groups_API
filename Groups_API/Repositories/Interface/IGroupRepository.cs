using Groups_API.Models.Domain;

namespace Groups_API.Repositories.Interface
{
    public interface IGroupRepository
    {
        Task<IEnumerable<Group>> GetAllGroups();
        Task AddGroup(Group group);
        Task<bool> Exists(int groupId);
        Task<Group> GetGroupById(int id);
        Task<List<Group>> GetAllGroupsWithMembershipsAndDebts();

    }
}

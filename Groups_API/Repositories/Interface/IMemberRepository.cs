using Groups_API.Models.Domain;

namespace Groups_API.Repositories.Interface
{
    public interface IMemberRepository
    {
        Task AddMemberToGroup(Member member, Group group);
        Task<bool> RemoveMemberFromGroup(int groupId, int memberId);
        Task<bool> HasOutstandingDebts(int groupId, int memberId);

    }
}

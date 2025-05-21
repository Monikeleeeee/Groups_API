namespace Groups_API.Models.Domain
{
    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<GroupMembership> GroupMemberships { get; set; }
    }
}

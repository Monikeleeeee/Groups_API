namespace Groups_API.Models.Domain
{
    public class Group
    {
        public int Id { get; set; }
        public string Title {  get; set; }
        public List<GroupMembership> GroupMemberships { get; set; } = new();
        public List<Debt> Debts { get; set; } = new();

    }
}

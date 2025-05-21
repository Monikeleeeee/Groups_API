namespace Groups_API.Models.Domain
{
    public class GroupMembership
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }

        public int MemberId { get; set; }
        public Member Member { get; set; }

        public double Balance {  get; set; }
    }
}

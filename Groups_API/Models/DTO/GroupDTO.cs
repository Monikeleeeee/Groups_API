namespace Groups_API.Models.DTO
{
    public class GroupDTO
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public double Balance { get; set; }

        public List<MemberDTO>? Members { get; set; }
    }
}

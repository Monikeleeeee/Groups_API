namespace Groups_API.Models.DTO
{
    public class DebtDTO
    {
        public int FromMemberId { get; set; }
        public string FromMemberName { get; set; }
        public int ToMemberId { get; set; }
        public string ToMemberName { get; set; }
        public double Amount { get; set; }
    }
}

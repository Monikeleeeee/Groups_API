namespace Groups_API.Models.Domain
{
    public class Debt
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public int DebtorId { get; set; }
        public Member Debtor { get; set; }

        public int CreditorId { get; set; }
        public Member Creditor { get; set; }

        public double Amount { get; set; } 
    }
}

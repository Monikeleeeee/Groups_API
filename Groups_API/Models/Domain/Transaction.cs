using Groups_API.Models.Enums;

namespace Groups_API.Models.Domain
{
    public class Transaction
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public Group Group { get; set; }

        public int PayerId { get; set; }
        public Member Payer { get; set; }

        public double TotalAmount { get; set; }
        public DateTime Date { get; set; }

        public SplitType SplitType { get; set; }

        public List<TransactionSplit> Splits { get; set; }
    }

}

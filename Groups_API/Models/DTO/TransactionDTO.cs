namespace Groups_API.Models.DTO
{
    public class TransactionDTO
    {
        public int Id { get; set; }
        public string PayerName { get; set; } = string.Empty;
        public double TotalAmount { get; set; }
        public DateTime Date { get; set; }
        public string SplitType { get; set; } = string.Empty;
    }
}

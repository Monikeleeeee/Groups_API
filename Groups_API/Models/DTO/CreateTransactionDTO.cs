using Groups_API.Models.Enums;

namespace Groups_API.Models.DTO
{
    public class CreateTransactionDTO
    {
        public int GroupId { get; set; }
        public int PayerId { get; set; }
        public double TotalAmount { get; set; }
        public SplitType SplitType { get; set; }

        public List<SplitInputDTO> Splits { get; set; }
    }
}

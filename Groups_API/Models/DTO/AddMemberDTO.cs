using System.ComponentModel.DataAnnotations;

namespace Groups_API.Models.DTO
{
    public class AddMemberDTO
    {
        [Required]
        public string MemberName { get; set; }
        public int GroupId { get; set; }
    }
}

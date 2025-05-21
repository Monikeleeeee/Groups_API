using Microsoft.AspNetCore.Mvc;
using Groups_API.Models.Domain;
using Groups_API.Models.DTO;
using Groups_API.Repositories.Interface;

namespace Groups_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberController : ControllerBase
    {
        private readonly IMemberRepository _memberRepo;
        private readonly IGroupRepository _groupRepo;

        public MemberController(IMemberRepository memberRepo, IGroupRepository groupRepo)
        {
            _memberRepo = memberRepo;
            _groupRepo = groupRepo;
        }

        [HttpPost]
        public async Task<IActionResult> AddMember([FromBody] AddMemberDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MemberName))
                return BadRequest("Member name is required.");

            var group = await _groupRepo.GetGroupById(dto.GroupId);
            if (group == null)
                return NotFound("Group not found.");

            var member = new Member { Name = dto.MemberName };
            await _memberRepo.AddMemberToGroup(member, group);

            return Ok();
        }
        [HttpDelete("groups/{groupId}/members/{memberId}")]
        public async Task<IActionResult> RemoveMemberFromGroup(int groupId, int memberId)
        {
            var group = await _groupRepo.GetGroupById(groupId);
            if (group == null)
                return NotFound("Group not found.");

            var hasDebts = await _memberRepo.HasOutstandingDebts(groupId, memberId);
            if (hasDebts)
                return BadRequest("Member cannot be removed: they have unsettled debts.");

            var success = await _memberRepo.RemoveMemberFromGroup(groupId, memberId);
            if (!success)
                return BadRequest("Member cannot be removed: they have unsettled debts.");

            return Ok("Member removed.");
        }
    }
}

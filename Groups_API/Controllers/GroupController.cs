using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Groups_API.Models.Domain;
using Groups_API.Models.DTO;
using Groups_API.Repositories.Interface;

namespace Groups_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupRepository _groupRepository;

        public GroupController(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDTO>>> GetAllGroups([FromQuery] int memberId)
        {
            var groups = await _groupRepository.GetAllGroupsWithMembershipsAndDebts();

            var groupDTOs = groups.Select(g =>
            {
                var memberBalance = g.Debts
                    .Where(d => d.CreditorId == memberId || d.DebtorId == memberId)
                    .Sum(d => d.CreditorId == memberId ? d.Amount : -d.Amount);

                return new GroupDTO
                {
                    Id = g.Id,
                    Title = g.Title,
                    Balance = memberBalance
                };
            });

            return Ok(groupDTOs);
        }
        [HttpPost]
        public async Task<ActionResult<GroupDTO>> CreateGroup(CreateGroupDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return BadRequest("Group title is required.");
            }

            var newGroup = new Group
            {
                Title = dto.Title
            };

            await _groupRepository.AddGroup(newGroup);

            var groupDto = new GroupDTO
            {
                Id = newGroup.Id,
                Title = newGroup.Title
            };

            return CreatedAtAction(nameof(GetGroupById), new { id = newGroup.Id }, groupDto);
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<GroupDTO>> GetGroupById(int id, [FromQuery] int memberId = 0)
        {
            var group = await _groupRepository.GetGroupById(id);
            if (group == null)
                return NotFound();

            var dto = new GroupDTO
            {
                Id = group.Id,
                Title = group.Title,
                Balance = memberId == 0
                    ? 0
                    : group.Debts
                        .Where(d => d.CreditorId == memberId || d.DebtorId == memberId)
                        .Sum(d => d.CreditorId == memberId ? d.Amount : -d.Amount),

                Members = group.GroupMemberships.Select(gm => new MemberDTO
                {
                    Id = gm.Member.Id,
                    Name = gm.Member.Name
                }).ToList()
            };

            return Ok(dto);
        }

    }
}

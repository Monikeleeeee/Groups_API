using Groups_API.Controllers;
using Groups_API.Models.Domain;
using Groups_API.Models.DTO;
using Groups_API.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Groups_API.Tests
{
    public class GroupControllerTests
    {
        private readonly Mock<IGroupRepository> _mockGroupRepo;
        private readonly GroupController _controller;

        public GroupControllerTests()
        {
            _mockGroupRepo = new Mock<IGroupRepository>();
            _controller = new GroupController(_mockGroupRepo.Object);
        }

        [Fact]
        public async Task GetAllGroups_ReturnsOk_WithGroupDTOsAndBalances()
        {
            int memberId = 1;
            var groups = new List<Group>
            {
                new Group
                {
                    Id = 1,
                    Title = "Group A",
                    Debts = new List<Debt>
                    {
                        new Debt { CreditorId = memberId, DebtorId = 2, Amount = 100 },
                        new Debt { CreditorId = 3, DebtorId = memberId, Amount = 50 }
                    }
                },
                new Group
                {
                    Id = 2,
                    Title = "Group B",
                    Debts = new List<Debt>()
                }
            };

            _mockGroupRepo.Setup(r => r.GetAllGroupsWithMembershipsAndDebts())
                .ReturnsAsync(groups);

            var result = await _controller.GetAllGroups(memberId);
            
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var groupDTOs = Assert.IsAssignableFrom<IEnumerable<GroupDTO>>(okResult.Value);
            Assert.Equal(2, groupDTOs.Count());

            var groupA = groupDTOs.First(g => g.Id == 1);
            Assert.Equal(50, groupA.Balance);

            var groupB = groupDTOs.First(g => g.Id == 2);
            Assert.Equal(0, groupB.Balance);
        }

        [Fact]
        public async Task CreateGroup_ReturnsBadRequest_WhenTitleIsEmpty()
        {
            
            var dto = new CreateGroupDTO { Title = " " };

            var result = await _controller.CreateGroup(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Group title is required.", badRequest.Value);
        }

        [Fact]
        public async Task CreateGroup_ReturnsCreatedGroupDTO_WhenSuccessful()
        {
            var dto = new CreateGroupDTO { Title = "New Group" };
            Group? savedGroup = null;

            _mockGroupRepo.Setup(r => r.AddGroup(It.IsAny<Group>()))
                .Returns<Group>(group =>
                {
                    savedGroup = group;
                    group.Id = 123;
                    return Task.CompletedTask;
                });

            
            var result = await _controller.CreateGroup(dto);

            
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var groupDto = Assert.IsType<GroupDTO>(createdResult.Value);
            Assert.Equal(123, groupDto.Id);
            Assert.Equal(dto.Title, groupDto.Title);
            Assert.NotNull(savedGroup);
            Assert.Equal(dto.Title, savedGroup.Title);
        }

        [Fact]
        public async Task GetGroupById_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            int groupId = 999;
            _mockGroupRepo.Setup(r => r.GetGroupById(groupId)).ReturnsAsync((Group?)null);
            
            var result = await _controller.GetGroupById(groupId);
            
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetGroupById_ReturnsGroupDTO_WithMembersAndBalance()
        {
            int groupId = 1;
            int memberId = 2;
            var group = new Group
            {
                Id = groupId,
                Title = "Test Group",
                Debts = new List<Debt>
                {
                    new Debt { CreditorId = memberId, DebtorId = 3, Amount = 100 },
                    new Debt { CreditorId = 4, DebtorId = memberId, Amount = 40 }
                },
                GroupMemberships = new List<GroupMembership>
                {
                    new GroupMembership { Member = new Member { Id = 2, Name = "Alice" } },
                    new GroupMembership { Member = new Member { Id = 3, Name = "Bob" } }
                }
            };
            _mockGroupRepo.Setup(r => r.GetGroupById(groupId)).ReturnsAsync(group);

            var result = await _controller.GetGroupById(groupId, memberId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<GroupDTO>(okResult.Value);

            Assert.Equal(groupId, dto.Id);
            Assert.Equal("Test Group", dto.Title);

            Assert.Equal(60, dto.Balance);

            Assert.NotNull(dto.Members);
            Assert.Equal(2, dto.Members.Count);
            Assert.Contains(dto.Members, m => m.Id == 2 && m.Name == "Alice");
            Assert.Contains(dto.Members, m => m.Id == 3 && m.Name == "Bob");
        }
    }
}

using Groups_API.Controllers;
using Groups_API.Models.Domain;
using Groups_API.Models.DTO;
using Groups_API.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Groups_API.Tests
{
    public class MemberControllerTests
    {
        private readonly Mock<IMemberRepository> _mockMemberRepo;
        private readonly Mock<IGroupRepository> _mockGroupRepo;
        private readonly MemberController _controller;

        public MemberControllerTests()
        {
            _mockMemberRepo = new Mock<IMemberRepository>();
            _mockGroupRepo = new Mock<IGroupRepository>();
            _controller = new MemberController(_mockMemberRepo.Object, _mockGroupRepo.Object);
        }

        [Fact]
        public async Task AddMember_ReturnsBadRequest_WhenMemberNameIsEmpty()
        {
            var dto = new AddMemberDTO { GroupId = 1, MemberName = "" };

            var result = await _controller.AddMember(dto);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Member name is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddMember_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            var dto = new AddMemberDTO { GroupId = 1, MemberName = "John" };
            _mockGroupRepo.Setup(r => r.GetGroupById(dto.GroupId)).ReturnsAsync((Group)null);

            var result = await _controller.AddMember(dto);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Group not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task AddMember_ReturnsOk_WhenMemberAddedSuccessfully()
        {
            var dto = new AddMemberDTO { GroupId = 1, MemberName = "John" };
            var group = new Group { Id = dto.GroupId, Title = "Test Group" };
            _mockGroupRepo.Setup(r => r.GetGroupById(dto.GroupId)).ReturnsAsync(group);
            _mockMemberRepo.Setup(r => r.AddMemberToGroup(It.IsAny<Member>(), group)).Returns(Task.CompletedTask);

            var result = await _controller.AddMember(dto);

            Assert.IsType<OkResult>(result);
            _mockMemberRepo.Verify(r => r.AddMemberToGroup(It.Is<Member>(m => m.Name == "John"), group), Times.Once);
        }

        [Fact]
        public async Task RemoveMemberFromGroup_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            int groupId = 1, memberId = 10;
            _mockGroupRepo.Setup(r => r.GetGroupById(groupId)).ReturnsAsync((Group)null);

            var result = await _controller.RemoveMemberFromGroup(groupId, memberId);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Group not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task RemoveMemberFromGroup_ReturnsBadRequest_WhenMemberHasDebts()
        {
            int groupId = 1, memberId = 10;
            var group = new Group { Id = groupId };
            _mockGroupRepo.Setup(r => r.GetGroupById(groupId)).ReturnsAsync(group);
            _mockMemberRepo.Setup(r => r.HasOutstandingDebts(groupId, memberId)).ReturnsAsync(true);

            var result = await _controller.RemoveMemberFromGroup(groupId, memberId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Member cannot be removed: they have unsettled debts.", badRequestResult.Value);
        }

        [Fact]
        public async Task RemoveMemberFromGroup_ReturnsBadRequest_WhenRemoveFails()
        {
            int groupId = 1, memberId = 10;
            var group = new Group { Id = groupId };
            _mockGroupRepo.Setup(r => r.GetGroupById(groupId)).ReturnsAsync(group);
            _mockMemberRepo.Setup(r => r.HasOutstandingDebts(groupId, memberId)).ReturnsAsync(false);
            _mockMemberRepo.Setup(r => r.RemoveMemberFromGroup(groupId, memberId)).ReturnsAsync(false);

            var result = await _controller.RemoveMemberFromGroup(groupId, memberId);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Member cannot be removed: they have unsettled debts.", badRequestResult.Value);
        }

        [Fact]
        public async Task RemoveMemberFromGroup_ReturnsOk_WhenMemberRemovedSuccessfully()
        {
            int groupId = 1, memberId = 10;
            var group = new Group { Id = groupId };
            _mockGroupRepo.Setup(r => r.GetGroupById(groupId)).ReturnsAsync(group);
            _mockMemberRepo.Setup(r => r.HasOutstandingDebts(groupId, memberId)).ReturnsAsync(false);
            _mockMemberRepo.Setup(r => r.RemoveMemberFromGroup(groupId, memberId)).ReturnsAsync(true);

            var result = await _controller.RemoveMemberFromGroup(groupId, memberId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Member removed.", okResult.Value);
        }
    }
}

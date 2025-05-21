using Groups_API.Controllers;
using Groups_API.Models.DTO;
using Groups_API.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Groups_API.Tests
{
    public class GroupMembershipControllerTests
    {
        private readonly Mock<IGroupMembershipRepository> _mockRepository;
        private readonly GroupMembershipController _controller;

        public GroupMembershipControllerTests()
        {
            _mockRepository = new Mock<IGroupMembershipRepository>();
            _controller = new GroupMembershipController(_mockRepository.Object);
        }

        [Fact]
        public async Task GetGroupDebts_ReturnsOk_WithDebts()
        {
            int groupId = 1;
            var debts = new List<DebtDTO>
            {
                new DebtDTO { FromMemberId = 1, FromMemberName = "Alice", ToMemberId = 2, ToMemberName = "Bob", Amount = 100 },
                new DebtDTO { FromMemberId = 2, FromMemberName = "Bob", ToMemberId = 3, ToMemberName = "Charlie", Amount = 50 }
            };
            _mockRepository.Setup(r => r.GetGroupDebtMatrix(groupId)).ReturnsAsync(debts);

            var result = await _controller.GetGroupDebts(groupId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedDebts = Assert.IsAssignableFrom<IEnumerable<DebtDTO>>(okResult.Value);
            Assert.Equal(2, ((List<DebtDTO>)returnedDebts).Count);
            Assert.Equal("Alice", ((List<DebtDTO>)returnedDebts)[0].FromMemberName);
            Assert.Equal("Bob", ((List<DebtDTO>)returnedDebts)[0].ToMemberName);
        }


        [Fact]
        public async Task SettleDebt_ReturnsOk_WhenDebtSettled()
        {
            int groupId = 1, fromMemberId = 2, toMemberId = 3;
            _mockRepository.Setup(r => r.SettleDebtAsync(groupId, fromMemberId, toMemberId))
                .ReturnsAsync(true);

            var result = await _controller.SettleDebt(groupId, fromMemberId, toMemberId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Debt settled successfully.", okResult.Value);
        }

        [Fact]
        public async Task SettleDebt_ReturnsBadRequest_WhenDebtNotFound()
        {
            int groupId = 1, fromMemberId = 2, toMemberId = 3;
            _mockRepository.Setup(r => r.SettleDebtAsync(groupId, fromMemberId, toMemberId))
                .ReturnsAsync(false);

            var result = await _controller.SettleDebt(groupId, fromMemberId, toMemberId);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Debt not found or already settled.", badRequest.Value);
        }

        [Fact]
        public async Task GetGroupTransactions_ReturnsOk_WithTransactions()
        {
            int groupId = 1;
            var transactions = new List<TransactionDTO>
            {
                new TransactionDTO
                {
                    Id = 101,
                    PayerName = "Alice",
                    TotalAmount = 150.0,
                    Date = new DateTime(2025, 5, 1),
                    SplitType = "Equal"
                },
                new TransactionDTO
                {
                    Id = 102,
                    PayerName = "Bob",
                    TotalAmount = 75.5,
                    Date = new DateTime(2025, 5, 2),
                    SplitType = "Exact"
                }
            };

            _mockRepository.Setup(r => r.GetGroupTransactionsAsync(groupId))
                .ReturnsAsync(transactions);

            var result = await _controller.GetGroupTransactions(groupId);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedTransactions = Assert.IsAssignableFrom<IEnumerable<TransactionDTO>>(okResult.Value);
            var list = returnedTransactions.ToList();

            Assert.Equal(2, list.Count);
            Assert.Equal("Alice", list[0].PayerName);
            Assert.Equal(150.0, list[0].TotalAmount);
            Assert.Equal(new DateTime(2025, 5, 1), list[0].Date);
            Assert.Equal("Equal", list[0].SplitType);
        }

    }
}

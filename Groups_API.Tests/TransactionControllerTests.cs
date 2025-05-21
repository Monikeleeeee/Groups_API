using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groups_API.Controllers;
using Groups_API.Models.Domain;
using Groups_API.Models.DTO;
using Groups_API.Models.Enums;
using Groups_API.Repositories.Interface;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Groups_API.Tests
{
    public class TransactionControllerTests
    {
        private readonly Mock<ITransactionRepository> _transactionRepoMock;
        private readonly Mock<IGroupRepository> _groupRepoMock;
        private readonly TransactionController _controller;

        public TransactionControllerTests()
        {
            _transactionRepoMock = new Mock<ITransactionRepository>();
            _groupRepoMock = new Mock<IGroupRepository>();

            _controller = new TransactionController(_transactionRepoMock.Object, _groupRepoMock.Object);
        }

        [Fact]
        public async Task CreateTransaction_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            
            var dto = new CreateTransactionDTO
            {
                GroupId = 1,
                PayerId = 1,
                TotalAmount = 100,
                SplitType = SplitType.Equal,
                Splits = new List<SplitInputDTO>()
            };

            _groupRepoMock.Setup(r => r.GetGroupById(dto.GroupId))
                .ReturnsAsync((Group)null);

            
            var result = await _controller.CreateTransaction(dto);

            
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Group not found", notFoundResult.Value);
        }

        [Fact]
        public async Task CreateTransaction_ReturnsNotFound_WhenPayerNotInGroup()
        {
            
            var group = new Group
            {
                Id = 1,
                GroupMemberships = new List<GroupMembership>()
            };

            var dto = new CreateTransactionDTO
            {
                GroupId = 1,
                PayerId = 99,
                TotalAmount = 100,
                SplitType = SplitType.Equal,
                Splits = new List<SplitInputDTO>()
            };

            _groupRepoMock.Setup(r => r.GetGroupById(dto.GroupId))
                .ReturnsAsync(group);

            
            var result = await _controller.CreateTransaction(dto);

            
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Payer not found", notFoundResult.Value);
        }

        [Fact]
        public async Task CreateTransaction_CreatesTransaction_WithEqualSplit()
        {
            
            var group = new Group
            {
                Id = 1,
                GroupMemberships = new List<GroupMembership>
                {
                    new GroupMembership { Member = new Member { Id = 1, Name = "Alice" } },
                    new GroupMembership { Member = new Member { Id = 2, Name = "Bob" } }
                }
            };

            var dto = new CreateTransactionDTO
            {
                GroupId = 1,
                PayerId = 1,
                TotalAmount = 100,
                SplitType = SplitType.Equal,
                Splits = new List<SplitInputDTO>()
            };

            _groupRepoMock.Setup(r => r.GetGroupById(dto.GroupId))
                .ReturnsAsync(group);

            _transactionRepoMock.Setup(r => r.CreateTransactionAsync(It.IsAny<Transaction>()))
                .ReturnsAsync(new Transaction { })
                .Verifiable();


            _transactionRepoMock.Setup(r => r.UpdateDebtsIncrementally(dto.GroupId, It.IsAny<Transaction>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            
            var result = await _controller.CreateTransaction(dto);

            
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Transaction created and debts recalculated", okResult.Value);

            _transactionRepoMock.Verify(r => r.CreateTransactionAsync(It.Is<Transaction>(t =>
                t.GroupId == dto.GroupId &&
                t.PayerId == dto.PayerId &&
                t.TotalAmount == dto.TotalAmount &&
                t.Splits.Count == 2 &&
                t.Splits.All(s => Math.Abs(s.Amount - 50) < 0.01) 
            )), Times.Once);

            _transactionRepoMock.Verify(r => r.UpdateDebtsIncrementally(dto.GroupId, It.IsAny<Transaction>()), Times.Once);
        }

        [Fact]
        public async Task CreateTransaction_ReturnsBadRequest_WhenPercentageSplitDoesNotSumTo100()
        {
            
            var group = new Group
            {
                Id = 1,
                GroupMemberships = new List<GroupMembership>
                {
                    new GroupMembership { Member = new Member { Id = 1 } },
                    new GroupMembership { Member = new Member { Id = 2 } }
                }
            };

            var dto = new CreateTransactionDTO
            {
                GroupId = 1,
                PayerId = 1,
                TotalAmount = 100,
                SplitType = SplitType.Percentage,
                Splits = new List<SplitInputDTO>
                {
                    new SplitInputDTO { MemberId = 1, Value = 40 },
                    new SplitInputDTO { MemberId = 2, Value = 50 } 
                }
            };

            _groupRepoMock.Setup(r => r.GetGroupById(dto.GroupId))
                .ReturnsAsync(group);

            
            var result = await _controller.CreateTransaction(dto);

            
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Percentages must add up to 100", badRequest.Value);
        }

        [Fact]
        public async Task CreateTransaction_ReturnsBadRequest_WhenSplitTypeInvalid()
        {
            
            var group = new Group
            {
                Id = 1,
                GroupMemberships = new List<GroupMembership>
                {
                    new GroupMembership { Member = new Member { Id = 1 } }
                }
            };

            var dto = new CreateTransactionDTO
            {
                GroupId = 1,
                PayerId = 1,
                TotalAmount = 100,
                SplitType = (SplitType)999,
                Splits = new List<SplitInputDTO>()
            };

            _groupRepoMock.Setup(r => r.GetGroupById(dto.GroupId))
                .ReturnsAsync(group);

            var result = await _controller.CreateTransaction(dto);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid split type", badRequest.Value);
        }
    }
}

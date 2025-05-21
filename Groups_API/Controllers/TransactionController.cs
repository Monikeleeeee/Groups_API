using Microsoft.AspNetCore.Mvc;
using Groups_API.Models.Domain;
using Groups_API.Models.DTO;
using Groups_API.Repositories.Interface;
using Groups_API.Models.Enums;
using Groups_API.Repositories.Implementation;

namespace Groups_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IGroupRepository _groupRepository;

        public TransactionController(ITransactionRepository transactionRepository, IGroupRepository groupRepository)
        {
            _transactionRepository = transactionRepository;
            _groupRepository = groupRepository;
        }


        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionDTO dto)
        {
            var group = await _groupRepository.GetGroupById(dto.GroupId);
            if (group == null) return NotFound("Group not found");

            var payer = group.GroupMemberships.Select(m => m.Member).FirstOrDefault(m => m.Id == dto.PayerId);
            if (payer == null) return NotFound("Payer not found");

            var transaction = new Transaction
            {
                GroupId = group.Id,
                PayerId = payer.Id,
                TotalAmount = dto.TotalAmount,
                Date = DateTime.UtcNow,
                SplitType = dto.SplitType,
                Splits = new List<TransactionSplit>()
            };

            var members = group.GroupMemberships.Select(m => m.Member).ToList();

            if (dto.SplitType == SplitType.Equal)
            {
                var share = Math.Round(dto.TotalAmount / members.Count(), 2);
                foreach (var member in members)
                {
                    transaction.Splits.Add(new TransactionSplit
                    {
                        MemberId = member.Id,
                        Amount = share
                    });
                }
            }
            else if (dto.SplitType == SplitType.Percentage)
            {
                var totalPercent = dto.Splits.Sum(s => s.Value);
                if (Math.Abs(totalPercent - 100.0) > 0.01) return BadRequest("Percentages must add up to 100");

                foreach (var split in dto.Splits)
                {
                    transaction.Splits.Add(new TransactionSplit
                    {
                        MemberId = split.MemberId,
                        Amount = Math.Round((split.Value / 100.0) * dto.TotalAmount, 2)
                    });
                }
            }
            else if (dto.SplitType == SplitType.Dynamic)
            {
                var totalSplit = dto.Splits.Sum(s => s.Value);
                if (Math.Abs(totalSplit - dto.TotalAmount) > 0.01) return BadRequest("Split amounts must match total");

                foreach (var split in dto.Splits)
                {
                    transaction.Splits.Add(new TransactionSplit
                    {
                        MemberId = split.MemberId,
                        Amount = Math.Round(split.Value, 2)
                    });
                }
            }
            else
            {
                return BadRequest("Invalid split type");
            }

            await _transactionRepository.CreateTransactionAsync(transaction);

            await _transactionRepository.UpdateDebtsIncrementally(dto.GroupId, transaction);

            return Ok("Transaction created and debts recalculated");
        }
    }

}

using Groups_API.Models;
using Groups_API.Models.DTO;
using Groups_API.Repositories.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
namespace Groups_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupMembershipController : ControllerBase
    {
        private readonly IGroupMembershipRepository _repository;

        public GroupMembershipController(IGroupMembershipRepository repository)
        {
            _repository = repository;
        }
        [HttpGet("{groupId}/debts")]
        public async Task<ActionResult<IEnumerable<DebtDTO>>> GetGroupDebts(int groupId)
        {
            var debts = await _repository.GetGroupDebtMatrix(groupId);
            return Ok(debts);
        }

        [HttpPost("{groupId}/settle")]
        public async Task<IActionResult> SettleDebt(
        int groupId,
        [FromQuery] int fromMemberId,
        [FromQuery] int toMemberId)
        {
            var success = await _repository.SettleDebtAsync(groupId, fromMemberId, toMemberId);

            if (!success)
                return BadRequest("Debt not found or already settled.");

            return Ok("Debt settled successfully.");
        }
        [HttpGet("{groupId}/transactions")]
        public async Task<ActionResult<IEnumerable<TransactionDTO>>> GetGroupTransactions(int groupId)
        {
            var transactions = await _repository.GetGroupTransactionsAsync(groupId);
            return Ok(transactions);
        }
    }
}

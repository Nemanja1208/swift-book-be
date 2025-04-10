using Application.BankAccounts.Commands.CreateBankAccount;
using Application.BankAccounts.Commands.DeleteBankAccount;
using Application.BankAccounts.Commands.UpdateBankAccount;
using Application.BankAccounts.Dtos;
using Application.BankAccounts.Queries.GetAllBankAccounts;
using Application.BankAccounts.Queries.GetBankAccountByUser;
using Application.BankAccounts.Queries.GetById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    [Route("api/bank-accounts")]
    [ApiController]
    public class BankAccountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BankAccountsController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _mediator.Send(new GetAllBankAccountsQuery()));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id) =>
            await _mediator.Send(new GetBankAccountByIdQuery(id)) is var result && result.IsSuccess
                ? Ok(result)
                : NotFound(result);

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(Guid userId) =>
            Ok(await _mediator.Send(new GetBankAccountsByUserIdQuery(userId)));

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBankAccountDto dto) =>
            await _mediator.Send(new CreateBankAccountCommand(dto)) is var result && result.IsSuccess
                ? Ok(result)
                : BadRequest(result);

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBankAccountDto dto) =>
            await _mediator.Send(new UpdateBankAccountCommand(id, dto)) is var result && result.IsSuccess
                ? Ok(result)
                : BadRequest(result);

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id) =>
            await _mediator.Send(new DeleteBankAccountCommand(id)) is var result && result.IsSuccess
                ? Ok(result)
                : BadRequest(result);
    }
}

using Application.BankAccounts.Commands.CreateBankAccount;
using Application.BankAccounts.Dtos;
using Domain.Models.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/bank-accounts")]
    [ApiController]
    public class BankAccountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BankAccountsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        //[HttpGet]
        //public async Task<ActionResult<OperationResult<List<BankAccountDtoResponse>>>> GetAllBankAccounts()
        //{
        //    var result = await _mediator.Send(new GetAllBankAccountsQuery());
        //    return Ok(result);
        //}

        //[HttpGet("{id:guid}")]
        //public async Task<ActionResult<OperationResult<BankAccountDtoResponse>>> Get(Guid id)
        //{
        //    var result = await _mediator.Send(new GetBankAccountByIdQuery(id));
        //    return Ok(result);
        //}

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OperationResult<BankAccountDtoResponse>>> Get(Guid id)
        {
            // Dummy return for now
            return Ok(OperationResult<BankAccountDtoResponse>.Success(new BankAccountDtoResponse
            {
                Id = id,
                AccountNumber = "DUMMY",
                OwnerName = "Test",
                Balance = 0,
                Currency = "USD",
                IsActive = true,
                OpenedAt = DateTime.UtcNow
            }));
        }

        [HttpPost]
        public async Task<ActionResult<OperationResult<BankAccountDtoResponse>>> Create(CreateBankAccountDto dto)
        {
            var result = await _mediator.Send(new CreateBankAccountCommand(dto));
            return result.IsSuccess ? CreatedAtAction(nameof(Get), new { id = result.Data?.Id }, result) : BadRequest(result);
        }

        //[HttpPut("{id:guid}")]
        //public async Task<ActionResult<OperationResult<BankAccountDtoResponse>>> Update(Guid id, UpdateBankAccountDto dto)
        //{
        //    var result = await _mediator.Send(new UpdateBankAccountCommand(id, dto));
        //    return result.IsSuccess ? Ok(result) : BadRequest(result);
        //}

        //[HttpDelete("{id:guid}")]
        //public async Task<ActionResult<OperationResult<bool>>> Delete(Guid id)
        //{
        //    var result = await _mediator.Send(new DeleteBankAccountCommand(id));
        //    return result.IsSuccess ? Ok(result) : NotFound(result);
        //}


    }
}

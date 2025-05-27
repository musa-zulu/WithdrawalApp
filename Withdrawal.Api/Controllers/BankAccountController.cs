using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using Withdrawal.Application.Interfaces;

namespace Withdrawal.Api.Controllers;

[ApiController]
[Route("api/bank")]
public class BankAccountController(IBankAccountService service) : ControllerBase
{
    private readonly IBankAccountService _service = service;


    [HttpPost("withdraw")]
    public async Task<IActionResult> WithdrawAsync([FromQuery] long accountId, [FromQuery] decimal amount, [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey)
    {
        var result = await _service.WithdrawAsync(accountId, amount, idempotencyKey);

        return result.Match(
            onValue: msg => Ok(msg),
            onError: errors => Problem(
                detail: string.Join("; ", errors.Select(e => e.Description)),
                statusCode: errors.Any(e => e.Type == ErrorType.NotFound) ? 404 : 400
            )
        );
    }
}

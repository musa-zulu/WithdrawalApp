using ErrorOr;

namespace Withdrawal.Application.Interfaces;

public interface IBankAccountService
{
    Task<ErrorOr<string>> WithdrawAsync(long accountId, decimal amount, Guid idempotencyKey);
}

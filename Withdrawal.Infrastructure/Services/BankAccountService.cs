using Dapper;
using ErrorOr;
using Microsoft.Extensions.Logging;
using System.Data;
using Withdrawal.Application.Interfaces;
using Withdrawal.Domain.Events;

namespace Withdrawal.Infrastructure.Services;

public class BankAccountService(IDbConnection db, IOutboxService outbox, ILogger<BankAccountService> logger) : IBankAccountService
{
    private readonly IDbConnection _db = db;
    private readonly IOutboxService _outbox = outbox;
    private readonly ILogger<BankAccountService> _logger = logger;

    private const string SqlSelectBalanceForUpdate = "SELECT balance FROM accounts WHERE id = @Id FOR UPDATE";
    private const string SqlUpdateAccountBalance = "UPDATE accounts SET balance = balance - @Amount WHERE id = @Id";
    private const string SqlSelectIdempotencyResult = "SELECT result FROM withdrawal_requests WHERE idempotency_key = @Key";
    private const string SqlInsertWithdrawalRequest = "INSERT INTO withdrawal_requests (idempotency_key, account_id, amount, result) VALUES (@Key, @AccountId, @Amount, @Result)";

    public async Task<ErrorOr<string>> WithdrawAsync(long accountId, decimal amount, Guid idempotencyKey)
    {
        _logger.LogInformation("Withdrawal requested. AccountId: {AccountId}, Amount: {Amount}, IdempotencyKey: {IdempotencyKey}", accountId, amount, idempotencyKey);

        using var transaction = _db.BeginTransaction();

        try
        {
            string? existing = await CheckIfRequestExist(idempotencyKey, transaction);

            if (existing is not null)
            {
                _logger.LogInformation("Idempotent withdrawal request detected. Returning existing result for IdempotencyKey: {IdempotencyKey}", idempotencyKey);
                return existing;
            }

            var balance = await _db.QueryFirstOrDefaultAsync<decimal?>(
                SqlSelectBalanceForUpdate,
                new { Id = accountId }, transaction);

            if (balance is null)
            {
                _logger.LogWarning("Account not found. AccountId: {AccountId}", accountId);
                return Error.NotFound("Account.NotFound", "Account not found.");
            }

            if (balance < amount)
            {
                _logger.LogWarning("Insufficient funds. AccountId: {AccountId}, Balance: {Balance}, Requested: {Amount}", accountId, balance, amount);
                await StoreIdempotencyResult("Insufficient funds.", idempotencyKey, accountId, amount, transaction);
                return Error.Validation("Account.InsufficientFunds", "Insufficient funds.");
            }

            await _db.ExecuteAsync(
                SqlUpdateAccountBalance,
                new { Amount = amount, Id = accountId }, transaction);

            var evt = new WithdrawalEvent(accountId, amount);
            await _outbox.EnqueueAsync(evt, transaction);

            await StoreIdempotencyResult("Withdrawal successful.", idempotencyKey, accountId, amount, transaction);

            transaction.Commit();
            _logger.LogInformation("Withdrawal successful. AccountId: {AccountId}, Amount: {Amount}", accountId, amount);
            return "Withdrawal successful.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during withdrawal. AccountId: {AccountId}, Amount: {Amount}, IdempotencyKey: {IdempotencyKey}", accountId, amount, idempotencyKey);
            transaction.Rollback();
            return Error.Failure("Withdrawal.Failed", "An error occurred during withdrawal.");
        }
    }

    private async Task<string?> CheckIfRequestExist(Guid idempotencyKey, IDbTransaction tx)
    {
        try
        {
            return await _db.QueryFirstOrDefaultAsync<string>(
                SqlSelectIdempotencyResult,
                new { Key = idempotencyKey }, tx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking idempotency for key: {IdempotencyKey}", idempotencyKey);
            throw;
        }
    }

    private async Task<int> StoreIdempotencyResult(string result, Guid key, long accountId, decimal amount, IDbTransaction transaction)
    {
        try
        {
            return await _db.ExecuteAsync(
                SqlInsertWithdrawalRequest,
                new
                {
                    Key = key,
                    AccountId = accountId,
                    Amount = amount,
                    Result = result
                }, transaction);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing idempotency result. Key: {Key}, AccountId: {AccountId}, Amount: {Amount}, Result: {Result}", key, accountId, amount, result);
            throw;
        }
    }
}

namespace Withdrawal.Domain.Events;

public class WithdrawalEvent(long accountId, decimal amount)
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public long AccountId { get; set; } = accountId;
    public decimal Amount { get; set; } = amount;
    public string Status { get; set; } = "SUCCESSFUL";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

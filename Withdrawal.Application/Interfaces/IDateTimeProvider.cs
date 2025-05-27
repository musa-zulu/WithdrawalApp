namespace Withdrawal.Application.Interfaces;

public interface IDateTimeProvider
{
    public DateTime UtcNow { get; }
}


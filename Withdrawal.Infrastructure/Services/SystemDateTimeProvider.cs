using Withdrawal.Application.Interfaces;

namespace Withdrawal.Infrastructure.Services;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}


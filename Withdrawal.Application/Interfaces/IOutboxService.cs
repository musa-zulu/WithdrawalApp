using System.Data;

namespace Withdrawal.Application.Interfaces;

public interface IOutboxService
{
    Task EnqueueAsync(object evt, IDbTransaction? tx = null);
}

using Dapper;
using System.Data;
using System.Text.Json;
using Withdrawal.Application.Interfaces;

namespace Withdrawal.Infrastructure.Services.Messaging;

public class OutboxService(IDbConnection db) : IOutboxService
{
    private readonly IDbConnection _db = db;
    private readonly string _insertSql =
        "INSERT INTO event_outbox (id, event_type, payload, occurred_at, processed) " +
        "VALUES (@Id, @Type, @Payload, @OccurredAt, false)";

    public Task EnqueueAsync(object evt, IDbTransaction? tx = null)
    {
        var json = JsonSerializer.Serialize(evt);

        return _db.ExecuteAsync(
            _insertSql,
            new
            {
                Id = Guid.NewGuid(),
                Type = evt.GetType().Name,
                Payload = json,
                OccurredAt = DateTime.UtcNow
            }, tx);
    }
}

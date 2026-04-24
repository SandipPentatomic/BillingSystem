using Microsoft.EntityFrameworkCore;
using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Infrastructure.Persistence;

namespace SubscriptionBilling.Infrastructure.Tests.Support;

public sealed class FakeClock : IClock
{
    public FakeClock(DateTime utcNow)
    {
        UtcNow = utcNow;
    }

    public DateTime UtcNow { get; set; }
}

public sealed record TestCommand(string Value) : ICommand<TestResponse>;

public sealed record TestIdempotentCommand(string Value, string IdempotencyKey) : ICommand<TestResponse>, IIdempotentRequest;

public sealed record TestQuery(string Value) : IQuery<TestResponse>;

public sealed record TestResponse(string Value);

public sealed class TestCommandHandler : ICommandHandler<TestCommand, TestResponse>
{
    public int CallCount { get; private set; }

    public Task<TestResponse> HandleAsync(TestCommand command, CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(new TestResponse($"handled:{command.Value}"));
    }
}

public sealed class TestIdempotentCommandHandler : ICommandHandler<TestIdempotentCommand, TestResponse>
{
    public int CallCount { get; private set; }

    public Task<TestResponse> HandleAsync(TestIdempotentCommand command, CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(new TestResponse($"handled:{command.Value}"));
    }
}

public sealed class TestQueryHandler : IQueryHandler<TestQuery, TestResponse>
{
    public int CallCount { get; private set; }

    public Task<TestResponse> HandleAsync(TestQuery query, CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(new TestResponse($"queried:{query.Value}"));
    }
}

public sealed class FakeIdempotencyStore : SubscriptionBilling.Application.Abstractions.Persistence.IIdempotencyStore
{
    private readonly Dictionary<string, string> _responses = [];

    public int GetResponseCallCount { get; private set; }
    public int SaveResponseCallCount { get; private set; }

    public Task<string?> GetResponseAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        GetResponseCallCount++;
        _responses.TryGetValue(idempotencyKey, out var response);
        return Task.FromResult(response);
    }

    public Task SaveResponseAsync(string idempotencyKey, string responseJson, CancellationToken cancellationToken)
    {
        SaveResponseCallCount++;
        _responses[idempotencyKey] = responseJson;
        return Task.CompletedTask;
    }

    public void Seed(string idempotencyKey, string responseJson)
    {
        _responses[idempotencyKey] = responseJson;
    }
}

public static class TestDbContextFactory
{
    public static BillingDbContext Create()
    {
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new BillingDbContext(options);
    }
}

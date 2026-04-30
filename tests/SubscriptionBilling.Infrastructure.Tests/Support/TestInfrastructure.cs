using Microsoft.EntityFrameworkCore;
using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Application.Abstractions.Persistence;
using SubscriptionBilling.Infrastructure.Persistence;
using System.Collections.Concurrent;

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

public sealed class SlowTestIdempotentCommandHandler : ICommandHandler<TestIdempotentCommand, TestResponse>
{
    public int CallCount { get; private set; }

    public async Task<TestResponse> HandleAsync(TestIdempotentCommand command, CancellationToken cancellationToken)
    {
        CallCount++;
        await Task.Delay(50, cancellationToken);
        return new TestResponse($"handled:{command.Value}");
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

public sealed class FakeIdempotencyStore : IIdempotencyStore
{
    private readonly Dictionary<string, string> _responses = [];
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new(StringComparer.Ordinal);

    public int AcquireCallCount { get; private set; }
    public int GetResponseCallCount { get; private set; }
    public int SaveResponseCallCount { get; private set; }

    public async Task<IAsyncDisposable> AcquireAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        AcquireCallCount++;
        var semaphore = _locks.GetOrAdd(idempotencyKey, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);
        return new FakeLease(semaphore);
    }

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

    private sealed class FakeLease : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public FakeLease(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public ValueTask DisposeAsync()
        {
            _semaphore.Release();
            return ValueTask.CompletedTask;
        }
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

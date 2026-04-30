using Microsoft.EntityFrameworkCore;
using SubscriptionBilling.Application.Abstractions.Clock;
using SubscriptionBilling.Application.Abstractions.Persistence;
using System.Collections.Concurrent;

namespace SubscriptionBilling.Infrastructure.Persistence;

public sealed class IdempotencyStore : IIdempotencyStore
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new(StringComparer.Ordinal);
    private readonly BillingDbContext _dbContext;
    private readonly IClock _clock;

    public IdempotencyStore(BillingDbContext dbContext, IClock clock)
    {
        _dbContext = dbContext;
        _clock = clock;
    }

    public async Task<IAsyncDisposable> AcquireAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        var semaphore = Locks.GetOrAdd(idempotencyKey, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken);

        return new Releaser(semaphore);
    }

    public async Task<string?> GetResponseAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        var processedCommand = await _dbContext.ProcessedCommands
            .AsNoTracking()
            .SingleOrDefaultAsync(command => command.IdempotencyKey == idempotencyKey, cancellationToken);

        return processedCommand?.ResponseJson;
    }

    public async Task SaveResponseAsync(string idempotencyKey, string responseJson, CancellationToken cancellationToken)
    {
        var processedCommand = await _dbContext.ProcessedCommands
            .SingleOrDefaultAsync(command => command.IdempotencyKey == idempotencyKey, cancellationToken);

        if (processedCommand is null)
        {
            await _dbContext.ProcessedCommands.AddAsync(
                ProcessedCommand.Create(idempotencyKey, responseJson, _clock.UtcNow),
                cancellationToken);
        }
        else
        {
            processedCommand.UpdateResponse(responseJson);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed class Releaser : IAsyncDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public Releaser(SemaphoreSlim semaphore)
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

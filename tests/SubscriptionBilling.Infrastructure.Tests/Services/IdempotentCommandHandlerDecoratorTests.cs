using System.Text.Json;
using SubscriptionBilling.Infrastructure.Persistence;
using SubscriptionBilling.Infrastructure.Services;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Services;

public sealed class IdempotentCommandHandlerDecoratorTests
{
    [Fact]
    public async Task HandleAsync_Returns_Cached_Response_When_Idempotency_Key_Exists()
    {
        var store = new FakeIdempotencyStore();
        store.Seed("command-key", JsonSerializer.Serialize(new TestResponse("cached"), new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        var innerHandler = new TestIdempotentCommandHandler();
        var decorator = new IdempotentCommandHandlerDecorator<TestIdempotentCommand, TestResponse>(innerHandler, store);

        var response = await decorator.HandleAsync(new TestIdempotentCommand("alpha", "command-key"), CancellationToken.None);

        Assert.Equal("cached", response.Value);
        Assert.Equal(0, innerHandler.CallCount);
        Assert.Equal(1, store.AcquireCallCount);
        Assert.Equal(1, store.GetResponseCallCount);
        Assert.Equal(0, store.SaveResponseCallCount);
    }

    [Fact]
    public async Task HandleAsync_Executes_Inner_Handler_And_Caches_Response_When_Key_Is_New()
    {
        var store = new FakeIdempotencyStore();
        var innerHandler = new TestIdempotentCommandHandler();
        var decorator = new IdempotentCommandHandlerDecorator<TestIdempotentCommand, TestResponse>(innerHandler, store);

        var response = await decorator.HandleAsync(new TestIdempotentCommand("beta", "new-key"), CancellationToken.None);

        Assert.Equal("handled:beta", response.Value);
        Assert.Equal(1, innerHandler.CallCount);
        Assert.Equal(1, store.AcquireCallCount);
        Assert.Equal(1, store.GetResponseCallCount);
        Assert.Equal(1, store.SaveResponseCallCount);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Cached_Response_Cannot_Be_Deserialized()
    {
        var store = new FakeIdempotencyStore();
        store.Seed("command-key", "null");
        var decorator = new IdempotentCommandHandlerDecorator<TestIdempotentCommand, TestResponse>(
            new TestIdempotentCommandHandler(),
            store);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            decorator.HandleAsync(new TestIdempotentCommand("alpha", "command-key"), CancellationToken.None));

        Assert.Contains("could not be deserialized", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_Serializes_Concurrent_Requests_With_Same_Idempotency_Key()
    {
        var store = new FakeIdempotencyStore();
        var innerHandler = new SlowTestIdempotentCommandHandler();
        var decorator = new IdempotentCommandHandlerDecorator<TestIdempotentCommand, TestResponse>(innerHandler, store);

        var firstTask = decorator.HandleAsync(new TestIdempotentCommand("gamma", "shared-key"), CancellationToken.None);
        var secondTask = decorator.HandleAsync(new TestIdempotentCommand("gamma", "shared-key"), CancellationToken.None);

        var responses = await Task.WhenAll(firstTask, secondTask);

        Assert.Equal(1, innerHandler.CallCount);
        Assert.Equal("handled:gamma", responses[0].Value);
        Assert.Equal("handled:gamma", responses[1].Value);
        Assert.Equal(1, store.SaveResponseCallCount);
    }
}

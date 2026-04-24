using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Infrastructure.Services;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Services;

public sealed class CommandDispatcherTests
{
    [Fact]
    public async Task SendAsync_Resolves_Handler_For_Non_Idempotent_Command()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestCommandHandler>();
        services.AddSingleton<ICommandHandler<TestCommand, TestResponse>>(provider => provider.GetRequiredService<TestCommandHandler>());
        using var serviceProvider = services.BuildServiceProvider();
        var store = new FakeIdempotencyStore();
        var dispatcher = new CommandDispatcher(serviceProvider, store);

        var response = await dispatcher.SendAsync(new TestCommand("alpha"), CancellationToken.None);

        Assert.Equal("handled:alpha", response.Value);
        Assert.Equal(0, store.GetResponseCallCount);
        Assert.Equal(0, store.SaveResponseCallCount);
        Assert.Equal(1, serviceProvider.GetRequiredService<TestCommandHandler>().CallCount);
    }

    [Fact]
    public async Task SendAsync_Returns_Cached_Response_For_Idempotent_Command()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestIdempotentCommandHandler>();
        services.AddSingleton<ICommandHandler<TestIdempotentCommand, TestResponse>>(provider => provider.GetRequiredService<TestIdempotentCommandHandler>());
        using var serviceProvider = services.BuildServiceProvider();
        var store = new FakeIdempotencyStore();
        store.Seed("cache-key", JsonSerializer.Serialize(new TestResponse("cached"), new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        var dispatcher = new CommandDispatcher(serviceProvider, store);

        var response = await dispatcher.SendAsync(new TestIdempotentCommand("alpha", "cache-key"), CancellationToken.None);

        Assert.Equal("cached", response.Value);
        Assert.Equal(1, store.GetResponseCallCount);
        Assert.Equal(0, store.SaveResponseCallCount);
        Assert.Equal(0, serviceProvider.GetRequiredService<TestIdempotentCommandHandler>().CallCount);
    }

    [Fact]
    public async Task SendAsync_Executes_Handler_And_Saves_Response_For_New_Idempotent_Command()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestIdempotentCommandHandler>();
        services.AddSingleton<ICommandHandler<TestIdempotentCommand, TestResponse>>(provider => provider.GetRequiredService<TestIdempotentCommandHandler>());
        using var serviceProvider = services.BuildServiceProvider();
        var store = new FakeIdempotencyStore();
        var dispatcher = new CommandDispatcher(serviceProvider, store);

        var response = await dispatcher.SendAsync(new TestIdempotentCommand("beta", "new-key"), CancellationToken.None);

        Assert.Equal("handled:beta", response.Value);
        Assert.Equal(1, store.GetResponseCallCount);
        Assert.Equal(1, store.SaveResponseCallCount);
        Assert.Equal(1, serviceProvider.GetRequiredService<TestIdempotentCommandHandler>().CallCount);
    }
}

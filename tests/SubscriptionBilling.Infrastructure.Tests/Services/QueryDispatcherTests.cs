using Microsoft.Extensions.DependencyInjection;
using SubscriptionBilling.Application.Abstractions.CQRS;
using SubscriptionBilling.Infrastructure.Services;
using SubscriptionBilling.Infrastructure.Tests.Support;

namespace SubscriptionBilling.Infrastructure.Tests.Services;

public sealed class QueryDispatcherTests
{
    [Fact]
    public async Task QueryAsync_Resolves_Handler_And_Returns_Response()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TestQueryHandler>();
        services.AddSingleton<IQueryHandler<TestQuery, TestResponse>>(provider => provider.GetRequiredService<TestQueryHandler>());
        using var serviceProvider = services.BuildServiceProvider();
        var dispatcher = new QueryDispatcher(serviceProvider);

        var response = await dispatcher.QueryAsync(new TestQuery("alpha"), CancellationToken.None);

        Assert.Equal("queried:alpha", response.Value);
        Assert.Equal(1, serviceProvider.GetRequiredService<TestQueryHandler>().CallCount);
    }
}

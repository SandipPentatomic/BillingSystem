using Microsoft.AspNetCore.Http;
using SubscriptionBilling.Api.Extensions;

namespace SubscriptionBilling.Api.Tests.Extensions;

public sealed class HttpContextExtensionsTests
{
    [Fact]
    public void GetIdempotencyKey_Returns_Header_Value_When_Present()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Idempotency-Key"] = "header-key";

        var key = httpContext.GetIdempotencyKey();

        Assert.Equal("header-key", key);
    }

    [Fact]
    public void GetIdempotencyKey_Generates_Key_When_Header_Is_Missing()
    {
        var httpContext = new DefaultHttpContext();

        var key = httpContext.GetIdempotencyKey();

        Assert.True(Guid.TryParseExact(key, "N", out _));
    }

    [Fact]
    public void GetIdempotencyKey_Generates_Key_When_Header_Is_Whitespace()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Idempotency-Key"] = "   ";

        var key = httpContext.GetIdempotencyKey();

        Assert.True(Guid.TryParseExact(key, "N", out _));
    }
}

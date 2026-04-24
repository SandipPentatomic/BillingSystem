using SubscriptionBilling.Domain.Abstractions;

namespace SubscriptionBilling.Domain.Tests.Abstractions;

public sealed class DomainExceptionTests
{
    [Fact]
    public void Creating_Domain_Exception_With_Message_Sets_Message()
    {
        var message = "Test error message";

        var exception = new DomainException(message);

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Domain_Exception_Is_Exception()
    {
        var exception = new DomainException("Test");

        Assert.IsAssignableFrom<Exception>(exception);
    }
}

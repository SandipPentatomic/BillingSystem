using SubscriptionBilling.Domain.Abstractions;
using SubscriptionBilling.Domain.ValueObjects;

namespace SubscriptionBilling.Domain.Tests.ValueObjects;

public sealed class EmailAddressTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("john.doe@company.co.uk")]
    [InlineData("user+tag@domain.org")]
    public void Creating_EmailAddress_With_Valid_Email_Succeeds(string validEmail)
    {
        var email = new EmailAddress(validEmail);

        Assert.Equal(validEmail.ToLowerInvariant(), email.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Creating_EmailAddress_With_Empty_Email_Throws_DomainException(string? emptyEmail)
    {
        var exception = Assert.Throws<DomainException>(() => new EmailAddress(emptyEmail!));

        Assert.Equal("Email address is required.", exception.Message);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    public void Creating_EmailAddress_With_Invalid_Email_Format_Throws_DomainException(string invalidEmail)
    {
        var exception = Assert.Throws<DomainException>(() => new EmailAddress(invalidEmail));

        Assert.Equal("Email address is invalid.", exception.Message);
    }

    [Fact]
    public void EmailAddress_Is_Trimmed()
    {
        var email = new EmailAddress("  test@example.com  ");

        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void EmailAddress_Is_Converted_To_Lowercase()
    {
        var email = new EmailAddress("Test@Example.COM");

        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void To_String_Returns_Email_Value()
    {
        var email = new EmailAddress("test@example.com");

        Assert.Equal("test@example.com", email.ToString());
    }

    [Fact]
    public void Two_EmailAddress_Objects_With_Same_Email_Are_Equal()
    {
        var email1 = new EmailAddress("test@example.com");
        var email2 = new EmailAddress("test@example.com");

        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Two_EmailAddress_Objects_Are_Case_Insensitive()
    {
        var email1 = new EmailAddress("Test@Example.COM");
        var email2 = new EmailAddress("test@example.com");

        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Two_EmailAddress_Objects_With_Different_Emails_Are_Not_Equal()
    {
        var email1 = new EmailAddress("test1@example.com");
        var email2 = new EmailAddress("test2@example.com");

        Assert.NotEqual(email1, email2);
    }

    [Fact]
    public void Equal_EmailAddress_Objects_Have_Same_HashCode()
    {
        var email1 = new EmailAddress("test@example.com");
        var email2 = new EmailAddress("test@example.com");

        Assert.Equal(email1.GetHashCode(), email2.GetHashCode());
    }
}

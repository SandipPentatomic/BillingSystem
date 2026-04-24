using Microsoft.EntityFrameworkCore;
using SubscriptionBilling.Domain.Aggregates;
using SubscriptionBilling.Domain.Enums;

namespace SubscriptionBilling.Infrastructure.Persistence;

public sealed class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public DbSet<Invoice> Invoices => Set<Invoice>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public DbSet<ProcessedCommand> ProcessedCommands => Set<ProcessedCommand>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(customer => customer.Id);
            entity.Property(customer => customer.Name).IsRequired().HasMaxLength(200);
            entity.Property(customer => customer.CreatedOnUtc).IsRequired();
            entity.Ignore(customer => customer.DomainEvents);

            var emailBuilder = entity.OwnsOne(customer => customer.Email);
            emailBuilder.Property(value => value.Value)
                .IsRequired()
                .HasMaxLength(256);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(subscription => subscription.Id);
            entity.Property(subscription => subscription.CustomerId).IsRequired();
            entity.Property(subscription => subscription.PlanName).IsRequired().HasMaxLength(200);
            entity.Property(subscription => subscription.Status)
                .HasConversion(
                    value => value.ToString(),
                    value => Enum.Parse<SubscriptionStatus>(value));
            entity.Property(subscription => subscription.CurrentPeriodStartUtc).IsRequired();
            entity.Property(subscription => subscription.NextBillingDateUtc).IsRequired();
            entity.Property(subscription => subscription.InitialInvoiceGenerated).IsRequired();
            entity.Property(subscription => subscription.ActivatedOnUtc).IsRequired();
            entity.Property(subscription => subscription.CancelledOnUtc);
            entity.Ignore(subscription => subscription.DomainEvents);

            var priceBuilder = entity.OwnsOne(subscription => subscription.Price);
            priceBuilder.Property(value => value.Amount).IsRequired();
            priceBuilder.Property(value => value.Currency).HasMaxLength(3).IsRequired();

            var billingCycleBuilder = entity.OwnsOne(subscription => subscription.BillingCycle);
            billingCycleBuilder.Property(value => value.Interval).IsRequired();
            billingCycleBuilder.Property(value => value.Unit)
                .HasConversion(
                    value => value.ToString(),
                    value => Enum.Parse<BillingIntervalUnit>(value))
                .IsRequired();
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(invoice => invoice.Id);
            entity.Property(invoice => invoice.CustomerId).IsRequired();
            entity.Property(invoice => invoice.SubscriptionId).IsRequired();
            entity.Property(invoice => invoice.Status)
                .HasConversion(
                    value => value.ToString(),
                    value => Enum.Parse<InvoiceStatus>(value));
            entity.Property(invoice => invoice.PeriodStartUtc).IsRequired();
            entity.Property(invoice => invoice.PeriodEndUtc).IsRequired();
            entity.Property(invoice => invoice.DueDateUtc).IsRequired();
            entity.Property(invoice => invoice.IssuedOnUtc).IsRequired();
            entity.Property(invoice => invoice.PaidOnUtc);
            entity.Property(invoice => invoice.PaymentMode)
                .HasConversion(
                    value => value.HasValue ? value.Value.ToString() : null,
                    value => string.IsNullOrWhiteSpace(value) ? null : Enum.Parse<PaymentMode>(value));
            entity.Ignore(invoice => invoice.DomainEvents);

            var amountBuilder = entity.OwnsOne(invoice => invoice.Amount);
            amountBuilder.Property(value => value.Amount).IsRequired();
            amountBuilder.Property(value => value.Currency).HasMaxLength(3).IsRequired();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(message => message.Id);
            entity.Property(message => message.Type).IsRequired();
            entity.Property(message => message.Content).IsRequired();
            entity.Property(message => message.OccurredOnUtc).IsRequired();
        });

        modelBuilder.Entity<ProcessedCommand>(entity =>
        {
            entity.HasKey(command => command.IdempotencyKey);
            entity.Property(command => command.ResponseJson).IsRequired();
            entity.Property(command => command.CreatedOnUtc).IsRequired();
        });
    }
}

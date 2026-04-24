# Subscription Billing System
 

Readme

## Overview

This service manages customers, subscriptions, and invoices.

Supported business capabilities:

- create customers
- create subscriptions
- generate the first invoice when a subscription is activated
- generate future invoices on each billing cycle
- pay invoices with a payment mode
- cancel subscriptions while preserving invoice history
- query invoices by customer, subscription, and status

The solution is intentionally split into domain, application, infrastructure, and API layers so business rules stay inside the domain model and transport/persistence concerns stay outside it.

## Solution Structure

- `src/SubscriptionBilling.Domain`
  Core business model: aggregates, value objects, enums, domain events, and domain exceptions
- `src/SubscriptionBilling.Application`
  CQRS contracts, command/query handlers, repository abstractions, and application orchestration
- `src/SubscriptionBilling.Infrastructure`
  EF Core persistence, repositories, idempotency store, outbox persistence, and hosted background services
- `src/SubscriptionBilling.Api`
  ASP.NET Core Web API, Swagger, controllers, request contracts, and startup wiring
- `tests/SubscriptionBilling.Domain.Tests`
  Unit tests for domain behavior

## Architecture

### Domain

The domain layer models the business directly:

- `Customer` as an aggregate root
- `Subscription` as an aggregate root
- `Invoice` as an aggregate root
- `Money`, `EmailAddress`, and `BillingCycle` as value objects

Business invariants are enforced inside the aggregates and value objects rather than in controllers.

Examples:

- a subscription cannot be created without a customer id and plan name
- the first invoice can only be generated once
- cancelled subscriptions stop generating future invoices
- an invoice cannot be paid twice

### Application

The application layer coordinates use cases through explicit commands and queries.

Examples:

- `CreateCustomerCommand`
- `CreateSubscriptionCommand`
- `CancelSubscriptionCommand`
- `PayInvoiceCommand`
- `GetInvoicesQuery`
- `RunBillingCycleCommand`

Handlers orchestrate repository access and call domain behavior, but they do not own the business rules themselves.

### Infrastructure

Infrastructure provides the technical implementation for:

- EF Core persistence
- repository implementations
- idempotency storage
- outbox persistence
- background hosted services

The current persistence provider is EF Core InMemory:

```csharp
options.UseInMemoryDatabase("SubscriptionBillingDb")
```

That keeps the project easy to run locally and suitable for the coding task.

### API

The API is controller-based and documented through Swagger.

Current controllers:

- `CustomersController`
- `SubscriptionsController`
- `InvoicesController`
- `BillingController`

## Domain Model Highlights

### Subscription lifecycle

- creating a subscription activates it immediately
- activation raises a `SubscriptionActivatedDomainEvent`
- the first invoice is generated at activation time
- future invoices are generated when the subscription becomes due
- cancelling a subscription stops future invoice generation

### Invoice lifecycle

- invoices are generated from subscription billing events
- invoices start in `Pending`
- payment transitions an invoice to `Paid`
- payment records a `PaymentMode`
- payment raises a `PaymentReceivedDomainEvent`

### Billing intervals

Subscriptions support these interval units:

- `Minutes`
- `Hours`
- `Days`
- `Months`

This allows realistic production-style monthly plans and also short testing cycles such as `1 Minute` or `15 Minutes`.

### Payment modes

Supported payment modes:

- `Cash`
- `Check`
- `Online`

The chosen payment mode is stored on the invoice and returned by invoice queries.

## Domain Events

The required events are modeled explicitly:

- `SubscriptionActivatedDomainEvent`
- `InvoiceGeneratedDomainEvent`
- `PaymentReceivedDomainEvent`

These are raised inside the domain model and persisted to the outbox during the unit-of-work save.

## Outbox and Background Processing

### Billing cycle background service

`BillingCycleBackgroundService` runs periodically and triggers the billing use case that generates invoices for subscriptions whose next billing date has passed.

### Outbox background service

`OutboxBackgroundService` polls unprocessed outbox messages, logs dispatch activity, and marks them as processed.

### Why the outbox exists

The outbox stores domain events durably alongside the main business transaction so asynchronous processing can happen after the aggregate state is saved.

In this implementation, the outbox is a lightweight version intended for the task:

- events are stored
- a background worker processes them
- dispatch is logged

It is not yet publishing to an external broker such as Kafka or RabbitMQ.

## Idempotency

Command endpoints support an `Idempotency-Key` header.

If a completed request is repeated with the same key, the previously stored response is returned instead of re-running the command.

Current limitation:

- this implementation protects completed retries
- it does not fully guard against concurrent in-flight duplicates arriving before the first request finishes

That tradeoff is acceptable for this task and is documented intentionally.

## API Endpoints

Recommended usage order:

1. `POST /api/customers`
2. `POST /api/subscriptions`
3. `GET /api/invoices`
4. `POST /api/invoices/{invoiceId}/pay`
5. `POST /api/subscriptions/{subscriptionId}/cancel`
6. `POST /api/billing/run`

Notes:

- `GET /api/invoices` is the main read endpoint for checking initial invoices, payment state, and billing-cycle results.
- `POST /api/billing/run` is mainly useful for demos and manual testing of the billing cycle behavior.

Supported invoice query filters:

- `customerId`
- `subscriptionId`
- `status`

## Example Requests

### Create customer

```http
POST /api/customers
Idempotency-Key: create-customer-001
Content-Type: application/json

{
  "name": "Alice Johnson",
  "email": "alice@example.com"
}
```

### Create subscription

```http
POST /api/subscriptions
Idempotency-Key: create-subscription-001
Content-Type: application/json

{
  "customerId": "PUT-CUSTOMER-ID-HERE",
  "planName": "Pro Monthly",
  "amount": 49.99,
  "currency": "USD",
  "billingInterval": 1,
  "billingIntervalUnit": "Months"
}
```

### Create fast-cycle subscription for testing

```http
POST /api/subscriptions
Idempotency-Key: create-fast-subscription-001
Content-Type: application/json

{
  "customerId": "PUT-CUSTOMER-ID-HERE",
  "planName": "Quick Test Plan",
  "amount": 5.00,
  "currency": "USD",
  "billingInterval": 1,
  "billingIntervalUnit": "Minutes"
}
```

### Pay invoice

```http
POST /api/invoices/PUT-INVOICE-ID-HERE/pay
Idempotency-Key: pay-invoice-001
Content-Type: application/json

{
  "paymentMode": "Online"
}
```

### Query invoices

```http
GET /api/invoices?status=Pending
```

## Running the Project

### Prerequisites

- .NET SDK installed

### Start the API

```powershell
dotnet run --project .\src\SubscriptionBilling.Api\SubscriptionBilling.Api.csproj
```

The API runs on:

```text
http://localhost:5009
```

Swagger UI:

```text
http://localhost:5009/swagger
```

### Build

```powershell
dotnet build .\SubscriptionBilling.slnx
```

### Run tests

```powershell
dotnet test .\tests\SubscriptionBilling.Domain.Tests\SubscriptionBilling.Domain.Tests.csproj
```

## Testing Notes

Useful manual test flow:

1. Create a customer
2. Create a `Minutes`-based subscription
3. Confirm the initial invoice exists
4. Pay the initial invoice using one of the supported payment modes
5. Wait for the interval to elapse
6. Trigger `POST /api/billing/run`
7. Query invoices again and confirm a new invoice was generated

## Design Decisions

- The domain model is behavior-rich to avoid an anemic design.
- CQRS is implemented without MediatR to keep the task focused and dependency-light.
- EF Core InMemory keeps startup friction low and makes the repository boundary easy to demonstrate.
- Controllers are thin and delegate work to the application layer.
- The outbox and hosted services are included because they were part of the requested bonus scope.
- Swagger is enabled for local developer ergonomics and easier review.

## Limitations

- Data is in-memory and is lost when the application stops.
- There is no external message broker behind the outbox.
- Idempotency is not fully protected against simultaneous in-flight duplicates.
- Authentication and authorization are intentionally out of scope for the task.

## What To Review First

If you are reviewing the solution, the most useful files to start with are:

- `src/SubscriptionBilling.Domain/Aggregates/Subscription.cs`
- `src/SubscriptionBilling.Domain/Aggregates/Invoice.cs`
- `src/SubscriptionBilling.Application/Features/Subscriptions/CreateSubscriptionCommandHandler.cs`
- `src/SubscriptionBilling.Infrastructure/Persistence/BillingDbContext.cs`
- `src/SubscriptionBilling.Infrastructure/Background/BillingCycleBackgroundService.cs`
- `src/SubscriptionBilling.Api/Controllers`
#   B i l l i n g S y s t e m 
 
 

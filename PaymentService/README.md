# OrderFlow Payment Service

An independent ASP.NET Core microservice that owns **payment processing** for OrderFlow. It has
its **own PostgreSQL database** and communicates with the Main Server over **HTTP/REST**. It never
connects to or queries the Main Server database.

## Ownership boundaries

| Main Server owns | Payment Service owns |
|------------------|----------------------|
| Users, Products, Orders, OrderItems, Authentication | Payments, payment transactions, payment status |

`OrderId` stored on a payment is only a **reference** to the Main Server's order id — there is no
cross-database foreign key.

## Technologies

- ASP.NET Core 9 Web API
- Entity Framework Core + PostgreSQL (Npgsql)
- FluentValidation
- Serilog structured logging
- Swagger / OpenAPI
- Dependency Injection, async/await, DTOs

## Project structure

```text
PaymentService/
├── Controllers/         # PaymentsController + ApiControllerBase
├── Services/            # IPaymentService + PaymentService (business logic)
├── DTOs/                # CreatePaymentRequest, PaymentResponse, Common/ApiResponse
├── Entities/            # Payment + Enums (PaymentStatus, PaymentProvider)
├── Data/                # PaymentDbContext + EF Configurations
├── Validators/          # CreatePaymentRequestValidator (FluentValidation)
├── Exceptions/          # NotFoundException
├── Middleware/          # GlobalExceptionHandler (consistent error responses)
├── Extensions/          # Clean Program.cs — DI/config split into extension methods
├── Providers/           # IPaymentProvider + MockPaymentProvider (swap in a real one later)
├── Helpers/             # ModelStateErrorMapper
├── Migrations/          # EF Core migrations
├── Program.cs
└── appsettings.json
```

## Database schema

Separate database: `orderflow_payments`.

```text
Payments
--------------------------------------------------
Id             integer        PK, identity
OrderId        integer        Main Server order id (indexed, NOT a FK)
Amount         numeric(18,2)
Currency       varchar(3)     ISO code, e.g. USD
Status         varchar(32)    Pending | Succeeded | Failed | Refunded
TransactionId  varchar(64)    nullable, set by the provider
Provider       varchar(32)    Mock | Stripe | PayPal
CreatedAt      timestamptz
UpdatedAt      timestamptz
```

Enums are stored as readable text. Status values: `Pending`, `Succeeded`, `Failed`, `Refunded`.

## API

Base URL (dev): `http://localhost:3100`

| Method | Route                          | Description                          |
|--------|--------------------------------|--------------------------------------|
| POST   | `/api/payments`                | Create + process a payment           |
| GET    | `/api/payments/{id}`           | Get a payment by id                  |
| GET    | `/api/payments/order/{orderId}`| List payments for a Main Server order|

### Create a payment

```http
POST /api/payments
Content-Type: application/json

{
  "orderId": 123,
  "amount": 150.00,
  "currency": "USD"
}
```

Response `201 Created`:

```json
{
  "success": true,
  "data": {
    "paymentId": 1,
    "orderId": 123,
    "amount": 150.00,
    "currency": "USD",
    "status": "Succeeded",
    "transactionId": "TXN-1A2B3C4D5E",
    "provider": "Mock",
    "createdAt": "2026-09-16T14:15:00Z",
    "updatedAt": "2026-09-16T14:15:00Z"
  }
}
```

### Get a payment

```http
GET /api/payments/1
```

### Get payments for an order

```http
GET /api/payments/order/123
```

### Error response (consistent shape)

Validation (`400`):

```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": {
    "currency": ["Currency must be a 3-letter ISO code (e.g. USD)."]
  }
}
```

Not found (`404`):

```json
{
  "success": false,
  "message": "Payment with id 999 was not found.",
  "errors": {}
}
```

## Payment flow

1. Validate the request (FluentValidation).
2. Create a `Payment` record with status `Pending`.
3. Process the charge through the configured `IPaymentProvider` (the demo uses `MockPaymentProvider`).
4. Update the status to `Succeeded` or `Failed` and store the `TransactionId`.
5. Return the payment result.

A declined charge is a normal outcome: the payment is persisted with status `Failed` and still
returned as `201` (it is not an error). The mock provider approves everything **except** amounts
over `10,000`, which it declines so the `Failed` path can be demonstrated.

### Plugging in a real provider

Implement `IPaymentProvider` (e.g. `StripePaymentProvider`) and register it in
`ServiceExtensions.AddApplicationServices`. No controller or service changes are required.

## Error handling

- No try/catch in controllers or services.
- A global `IExceptionHandler` (`GlobalExceptionHandler`) maps exceptions to consistent JSON:
  - `ValidationException` / invalid model state → `400`
  - `NotFoundException` → `404`
  - anything else → `500`

## How to run

### 1. Create the database

The service uses its own database `orderflow_payments`. Create it in your local PostgreSQL:

```bash
createdb orderflow_payments
# or:  psql -U postgres -c "CREATE DATABASE orderflow_payments;"
```

Adjust `PaymentService/appsettings.Development.json` if your credentials differ. Default dev
connection:

```text
Host=localhost;Port=5432;Database=orderflow_payments;Username=root;Password=123456
```

### 2. Apply migrations and run

```bash
dotnet ef database update --project PaymentService
dotnet run --project PaymentService
```

Migrations are also applied automatically on startup. Swagger UI: `http://localhost:3100/swagger`.

## Design decisions

- **Independent database** — separate `orderflow_payments`; no FK to the Main Server.
- **HTTP/REST integration** — the Main Server calls `POST /api/payments`; no shared DB, queues, or events.
- **Provider abstraction** — `IPaymentProvider` isolates the (mock) provider so a real one drops in later.
- **DbContext in the service** — no repository layer; the domain is a single aggregate (Payment).
- **Global exception handling + FluentValidation** — consistent responses and correct status codes.

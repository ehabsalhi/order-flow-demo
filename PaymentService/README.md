# OrderFlow Payment Service

An independent ASP.NET Core microservice that owns **payment processing** for OrderFlow. It has
its **own PostgreSQL database** and never connects to or queries the Main Server database.

It communicates in two directions:

- **Synchronous** command in: the Main Server calls the service (REST **and** gRPC) to charge an
  order and gets the result back immediately.
- **Asynchronous** events out: after a charge completes, the service publishes
  `payment.succeeded` / `payment.failed` events to **RabbitMQ** (via a transactional outbox) so
  other services can react without being coupled to the payment call.

## Ownership boundaries

| Main Server owns | Payment Service owns |
|------------------|----------------------|
| Users, Products, Orders, OrderItems, Authentication | Payments, payment transactions, payment status |

`OrderId` stored on a payment is only a **reference** to the Main Server's order id — there is no
cross-database foreign key.

## Technologies

- ASP.NET Core 9 Web API
- Entity Framework Core + PostgreSQL (Npgsql)
- gRPC (`Grpc.AspNetCore`) for the synchronous service-to-service command
- RabbitMQ (`RabbitMQ.Client`) for asynchronous events, with a transactional outbox
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
├── Exceptions/          # NotFoundException, ConflictException
├── Middleware/          # GlobalExceptionHandler (consistent error responses)
├── Extensions/          # Clean Program.cs — DI/config split into extension methods
├── Providers/           # IPaymentProvider + MockPaymentProvider (swap in a real one later)
├── Grpc/                # PaymentGrpcService + GrpcExceptionInterceptor
├── Protos/              # payments.proto (gRPC contract)
├── Messaging/           # IEventPublisher + RabbitMQ publisher, outbox background service, event
├── Helpers/             # ModelStateErrorMapper, PaginationHelper
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
Amount         numeric
Currency       text           ISO code, e.g. USD
Status         text           Pending | Succeeded | Failed | Refunded
TransactionId  varchar(64)    nullable, set by the provider
Provider       text           Mock | Stripe | PayPal
CreatedAt      timestamptz
UpdatedAt      timestamptz

OutboxMessages
--------------------------------------------------
Id             uuid           PK
Type           varchar(128)   event/routing key, e.g. payment.succeeded
Payload        text           serialized event JSON
CreatedAt      timestamptz
PublishedAt    timestamptz    nullable, set once sent to RabbitMQ (indexed)
RetryCount     integer
```

Enums (`Status`, `Provider`) are stored as readable text. Status values: `Pending`,
`Succeeded`, `Failed`, `Refunded`.

## API (REST)

Base URL (dev): `http://localhost:3100`

| Method | Route                          | Description                                  |
|--------|--------------------------------|----------------------------------------------|
| POST   | `/api/payments`                | Create + process a payment                   |
| GET    | `/api/payments/{id}`           | Get a payment by id                          |
| GET    | `/api/payments/order/{orderId}`| List payments for an order (paginated)       |

`GET /api/payments/order/{orderId}` accepts `?page=` and `?pageSize=` query params
(defaults `1` / `10`, max page size `100`) and returns a paginated envelope.

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

Conflict (`409`) — the order already has a successful payment (prevents double-charging):

```json
{
  "success": false,
  "message": "Order 123 has already been paid successfully.",
  "errors": {}
}
```

## Synchronous communication (gRPC)

Alongside REST, the service exposes a gRPC endpoint for the Main Server's "charge this order"
command. The contract is in `Protos/payments.proto`:

```proto
service Payments {
  rpc CreatePayment (CreatePaymentGrpcRequest) returns (PaymentGrpcReply);
}
```

- gRPC listens on its own HTTP/2 endpoint (dev: `http://localhost:3101`, cleartext h2c).
- `amount` is sent as a string to preserve decimal precision.
- Errors map to gRPC status codes via `GrpcExceptionInterceptor`: validation → `InvalidArgument`,
  not found → `NotFound`, conflict → `AlreadyExists`.

Both REST and gRPC call the same `IPaymentService`, so the business logic is shared.

## Asynchronous events (RabbitMQ + transactional outbox)

When a charge completes, the service records the final state **and** an outbox row in the **same
database transaction**, then a background worker relays it to RabbitMQ. This guarantees an event is
never lost even if the broker is momentarily down.

- Event: `PaymentStatusChangedEvent` (paymentId, orderId, status, transactionId, amount, currency,
  provider, occurredAt).
- Exchange: `payments` (topic, durable). Routing key: `payment.succeeded` / `payment.failed`.
- Reliability: `OutboxPublisherService` polls unpublished rows, publishes, and stamps `PublishedAt`;
  failures increment `RetryCount` and are retried on the next cycle.

If RabbitMQ is unavailable the API still works — events simply queue in the `OutboxMessages` table
and drain once the broker is reachable.

## Payment flow

1. Validate the request (FluentValidation).
2. Reject with `409` if the order already has a `Succeeded` payment (idempotency guard).
3. Create a `Payment` record with status `Pending`.
4. Process the charge through the configured `IPaymentProvider` (the demo uses `MockPaymentProvider`).
5. Update the status to `Succeeded` or `Failed`, store the `TransactionId`, and write a
   `payment.*` event to the outbox — all in one `SaveChanges`.
6. Return the payment result. The outbox worker publishes the event to RabbitMQ shortly after.

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
  - `ConflictException` → `409`
  - anything else → `500`
- The gRPC surface mirrors this via `GrpcExceptionInterceptor` (no try/catch in the gRPC service).

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

### 2. (Optional) Start RabbitMQ

Only needed to actually deliver async events. The API works without it — events queue in the
outbox until the broker is reachable.

```bash
docker run -d --name orderflow-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

Management UI: `http://localhost:15672` (guest / guest). Adjust the `RabbitMq` section in
`appsettings.json` if your broker differs.

### 3. Apply migrations and run

```bash
dotnet ef database update --project PaymentService
dotnet run --project PaymentService
```

Migrations are also applied automatically on startup.

Endpoints (dev):

- REST + Swagger UI: `http://localhost:3100/swagger`
- gRPC (HTTP/2): `http://localhost:3101`

## Design decisions

- **Independent database** — separate `orderflow_payments`; no FK to the Main Server.
- **Sync command via REST + gRPC** — the Main Server charges an order and gets the result back.
- **Async events via RabbitMQ + outbox** — status changes are published reliably, decoupling the
  Order service from the payment call.
- **Idempotency guard** — a second charge for an already-paid order returns `409`, preventing
  double-charging.
- **Provider abstraction** — `IPaymentProvider` isolates the (mock) provider so a real one drops in later.
- **DbContext in the service** — no repository layer; the domain is a single aggregate (Payment).
- **Global exception handling + FluentValidation** — consistent responses and correct status codes.

# OrderFlow

An e-commerce demo that splits **orders**, **payments**, and **notifications** into separate services, each with its own PostgreSQL database. The client talks only to the Main Server.

## Run with Docker

From the repo root:

```bash
Run:
docker compose -f docker/docker-compose.yml up --build -d

Stop:
docker compose -f docker/docker-compose.yml down
```

| What                    | URL                                                              |
| ----------------------- | ---------------------------------------------------------------- |
| Main Server Swagger     | [http://localhost:3000/swagger](http://localhost:3000/swagger)   |
| Payment Service Swagger | [http://localhost:3100/swagger](http://localhost:3100/swagger)   |
| Notification Swagger    | [http://localhost:3200/swagger](http://localhost:3200/swagger)   |
| RabbitMQ UI             | [http://localhost:15672](http://localhost:15672) (guest / guest) |
| Postgres                | `localhost:5432`                                                 |

Demo login:

User Account: `customer@demo.local` / `Customer123!`

Admin Account: `admin@demo.local` / `Admin123!`

## What it does

```text
Client → Main Server (auth, products, orders)
              │
              ├── gRPC (sync)  → Payment Service  "charge this order now"
              └── RabbitMQ (async) ← Payment Service  "payment succeeded / failed"
                         ├── Main Server        → order Paid / Cancelled
                         └── Notification Service → store + mock send
```

1. User creates an order on the Main Server.
2. Main Server charges via **gRPC** and waits for Succeeded / Failed.
3. Payment Service saves the payment and an outbox row in one DB transaction.
4. A worker publishes `payment.succeeded` / `payment.failed` to **RabbitMQ**.
5. Main Server consumes the event and sets the order to Paid or Cancelled.
6. Notification Service also consumes `payment.*`, saves a notification, and a mock sender “sends” it. Admins can list notifications through the Main Server HTTP API.

## Services

**Main Server** — public API. Owns users, products, categories, orders, JWT auth. Does not store payments or notifications. Calls Payment Service to charge (gRPC) and to read payments (HTTP); updates order status from `payment.*` events. Proxies the admin notification list over HTTP.

**Payment Service** — owns payments. Own database. gRPC for charging, HTTP for reads (`GET /api/payments/...`). Writes the payment and an outbox row in one commit; a worker publishes `payment.succeeded` / `payment.failed` to RabbitMQ. Main Server is the only caller. Notification Service does not call it.

**Notification Service** — owns notification records. Own database. Listens to `payment.*` only (no gRPC, no calls to Payment Service). Saves a notification and a mock sender “sends” it. One admin HTTP list API (`?orderId=` `&status=`); Main Server proxies it with JWT.

## Why three databases

This is **data ownership**: the service that owns a concept is the only one allowed to write (and usually read) that data.

- Main Server owns orders → database `orderflow`
- Payment Service owns payments → database `orderflow_payments`
- Notification Service owns notifications → database `orderflow_notifications`

They do **not** share tables or foreign keys across databases. `OrderId` on a payment is just a number, not a FK to `orderflow`. If the Payment Service queried the Main Server DB, it would no longer be independent: schema changes, downtime, and deploys would couple both services.

Each service can migrate, scale, and fail on its own. They talk only over APIs and events (gRPC / HTTP / RabbitMQ), not by joining each other's tables.

## Why gRPC, RabbitMQ, and the outbox

**gRPC** — used for **charging**. it's typed, fast, and meant for service-to-service calls.

**RabbitMQ** — used **after** the charge. The Payment Service publishes `payment.succeeded` / `payment.failed`. Main Server and Notification Service each bind their own queue to `payment.*`. If a consumer is down, the message waits in that queue.

**Outbox table** (`OutboxMessages` in the payments DB) — Postgres and RabbitMQ cannot share one transaction. Without an outbox:

```text
1. Save payment = Succeeded   ✅
2. App or RabbitMQ dies
3. Event never published      ❌  → order stays Pending forever
```

So the Payment Service writes the payment **and** the event row in the **same** database commit. A background worker then sends the row to RabbitMQ and sets `PublishedAt`. If RabbitMQ is down, the API still works; events drain when the broker is back. Retry cap parks a message after 5 failed publishes (`RetryCount`) instead of looping forever.

HTTP is used for **reads** (`GET /api/payments/...`) because pagination and Swagger fit REST.

## Structure

```text
MainServer/            public API + gRPC/HTTP clients + RabbitMQ consumer
PaymentService/        payments API + gRPC server + outbox publisher
NotificationService/   notifications API + RabbitMQ consumer (payment.*)
docker/                compose, Dockerfiles, Postgres init
```

Main Server: `Controllers` → `Services` → `Repositories` (own DB) or `Integrations/Payments` / `Integrations/Notifications` (other services).

Payment Service: `Controllers` / `Grpc` → `Services` → `PaymentDbContext`. Events go through `Messaging` (outbox → RabbitMQ).

Notification Service: `Controllers` → `Services` → `NotificationDbContext`. `Messaging` consumes `payment.*`.

## Stack (and why)

- **ASP.NET Core 9** — APIs
- **EF Core + PostgreSQL** — one database per service (independent ownership)
- **JWT** — Main Server auth
- **FluentValidation** — input validation
- **gRPC** — sync charge between services
- **RabbitMQ + outbox** — async status events without losing messages if the broker is down
- **Swagger** — try the APIs
- **Docker Compose** — run everything together

## Useful APIs (Main Server)

| Method | Route                           | Who                                 |
| ------ | ------------------------------- | ----------------------------------- |
| POST   | `/api/auth/login`               | Public                              |
| POST   | `/api/orders`                   | Logged in (creates order + charges) |
| GET    | `/api/orders/{id}`              | Owner or admin                      |
| GET    | `/api/payments/{id}`            | Owner or admin                      |
| GET    | `/api/payments/order/{orderId}` | Owner or admin                      |
| GET    | `/api/notifications`            | Admin (`?orderId=` `&status=`)      |

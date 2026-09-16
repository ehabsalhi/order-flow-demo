# OrderFlow Main Server

ASP.NET Core Web API main server for a small e-commerce / order management demo. This project owns authentication, users, products, categories, orders, and order items in a single PostgreSQL database.

Payment and Notification services are intentionally **outside** this repository and can be integrated later.

## Architecture

```text
Client (Swagger / Frontend)
        |
        v
ASP.NET Core Main Server
        |
        v
   PostgreSQL
```

## Technologies

- ASP.NET Core 9 Web API
- Entity Framework Core + PostgreSQL
- JWT Bearer authentication
- FluentValidation
- ASP.NET Core `PasswordHasher` for password hashing
- Serilog structured logging
- Swagger / OpenAPI (with persistent JWT authorization)

## Project structure

```text
MainServer/
├── Controllers/
├── Services/
├── Entities/
├── DTOs/
├── Validators/
├── Data/
├── Exceptions/
├── Middleware/
├── Extensions/
├── Mappings/
├── Helpers/
├── Migrations/
├── Program.cs
└── appsettings.json
```

## Prerequisites

- .NET 9 SDK
- PostgreSQL 16 (local install or Docker)

## How to run

### 1. Start PostgreSQL

Using Docker:

```bash
docker compose up -d
```

Default connection (also in `appsettings.Development.json`):

```text
Host=localhost;Port=5432;Database=orderflow;Username=postgres;Password=postgres
```

### 2. Restore, migrate, and run

```bash
dotnet restore
dotnet ef database update --project MainServer
dotnet run --project MainServer
```

Swagger UI: `http://localhost:5111/swagger`

On first run in **Development**, the app applies migrations and seeds demo data automatically.

## Development credentials (dev only)

| Role     | Email               | Password      |
|----------|---------------------|---------------|
| Admin    | admin@demo.local    | Admin123!     |
| Customer | customer@demo.local | Customer123!  |

Do not use these credentials in production.

## Authentication

1. **Register** a customer: `POST /api/auth/register`
2. **Login**: `POST /api/auth/login` — copy the JWT from the response
3. In Swagger, click **Authorize**
4. Enter: `Bearer {your-token}`
5. Call protected endpoints

Swagger is configured with `PersistAuthorization = true`, so the token remains in browser storage after closing/reopening the Swagger tab until you remove it or it expires.

## Authorization summary

| Role     | Permissions |
|----------|-------------|
| Public   | Browse products and categories |
| Customer | Create orders, view/cancel own orders |
| Admin    | Manage products/categories, view all orders |

**Cancellation rule:** only `Pending` orders can be cancelled. Stock is restored on cancel.

## API endpoints

### Auth

| Method | Route                | Auth   |
|--------|----------------------|--------|
| POST   | /api/auth/register   | Public |
| POST   | /api/auth/login      | Public |

### Products

| Method | Route                 | Auth        |
|--------|-----------------------|-------------|
| GET    | /api/products         | Public      |
| GET    | /api/products/{id}    | Public      |
| POST   | /api/products         | Admin       |
| PUT    | /api/products/{id}    | Admin       |
| DELETE | /api/products/{id}    | Admin       |

Query params for listing: `page`, `pageSize`, `search`, `categoryId`, `sortBy` (`name`|`price`|`createdAt`), `sortDirection` (`asc`|`desc`).

### Categories

| Method | Route                   | Auth   |
|--------|-------------------------|--------|
| GET    | /api/categories         | Public |
| GET    | /api/categories/{id}    | Public |
| POST   | /api/categories         | Admin  |
| PUT    | /api/categories/{id}    | Admin  |
| DELETE | /api/categories/{id}    | Admin  |

### Orders

| Method | Route                      | Auth              |
|--------|----------------------------|-------------------|
| POST   | /api/orders                    | Authenticated     |
| GET    | /api/orders/{id}               | Owner or Admin    |
| GET    | /api/orders/my-orders          | Authenticated     |
| PUT    | /api/orders/{id}/cancel        | Owner or Admin    |
| GET    | /api/payments/{id}             | Owner or Admin    |
| GET    | /api/payments/order/{orderId}  | Owner or Admin    |
| GET    | /api/admin/orders          | Admin             |

Admin order filters: `status`, `userId`, `fromDate`, `toDate`, `page`, `pageSize`.

## Example requests

### Register

```json
POST /api/auth/register
{
  "firstName": "Jane",
  "lastName": "Doe",
  "email": "jane@example.com",
  "password": "Password123"
}
```

### Login

```json
POST /api/auth/login
{
  "email": "customer@demo.local",
  "password": "Customer123!"
}
```

Response:

```json
{
  "success": true,
  "data": {
    "token": "eyJ...",
    "expiresAt": "2026-09-15T18:00:00Z",
    "userId": 2,
    "email": "customer@demo.local",
    "role": "Customer"
  }
}
```

### Create order

```json
POST /api/orders
Authorization: Bearer {token}
{
  "items": [
    { "productId": 1, "quantity": 2 }
  ]
}
```

The server calculates `unitPrice`, `totalAmount`, and stores product name/price snapshots on order items.

### Error response

```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": {
    "email": ["Email is required."]
  }
}
```

## Database schema

```text
Users
  Id, FirstName, LastName, Email, PasswordHash, Role, CreatedAt, UpdatedAt, IsDeleted

Categories
  Id, Name, Description, CreatedAt, UpdatedAt

Products
  Id, Name, Description, Price, Stock, CategoryId, CreatedAt, UpdatedAt

Orders
  Id, UserId, Status, TotalAmount, CreatedAt, UpdatedAt

OrderItems
  Id, OrderId, ProductId, ProductName, UnitPrice, Quantity
```

Relationships:

```text
User 1 ── * Orders 1 ── * OrderItems * ── 1 Product
Category 1 ── * Products
```

## Configuration

Set secrets via environment variables or user secrets — do not commit production secrets.

```bash
dotnet user-secrets init --project MainServer
dotnet user-secrets set "JwtSettings:Secret" "your-secure-secret-at-least-32-characters" --project MainServer
```

## Key design decisions

- **Single project, service layer + DbContext** — no unnecessary repository/CQRS layers
- **Global exception handler** — consistent JSON errors, no try/catch in every controller
- **Server-side order totals** — client sends only product IDs and quantities
- **Order item snapshots** — historical prices preserved when products change
- **Transactional order creation** — order, items, and stock updates commit or roll back together
- **Public catalog reads** — products/categories browsable without login; mutations require Admin

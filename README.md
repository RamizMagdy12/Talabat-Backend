# 🛒 Talabat API — E-Commerce Backend

A production-oriented **ASP.NET Core Web API** powering a full-featured e-commerce platform with secure JWT authentication, Redis-backed baskets, and end-to-end **Stripe** payment processing. Buyer identity is resolved exclusively from **JWT claims**—never from query strings or request bodies—ensuring a consistent and secure order lifecycle.

---

## ✨ Core Features

- 🔐 **JWT Authentication** — ASP.NET Core Identity with bearer tokens; buyer email resolved via `ClaimTypes.Email` for orders and payments
- 🛍️ **Product Catalog** — Filterable, sortable, and paginated product queries using the Specification pattern
- 🧺 **Redis Basket** — Persistent shopping baskets with live price synchronization
- 📦 **Order Management** — Order creation, user-scoped retrieval, delivery methods, and status tracking (`Pending`, `PaymentReceived`, `PaymentFailed`)
- 💳 **Stripe Payments** — PaymentIntent creation with automatic Stripe Customer registration and linking
- 🔔 **Stripe Webhooks** — Anonymous webhook endpoint updates order status strictly by `PaymentIntentId` (no JWT required)
- ⚡ **Response Caching** — Redis-backed caching for static/read-heavy endpoints with user-aware cache keys
- 🏗️ **Clean Architecture** — Separation of concerns across API, Core, Services, and Repository layers

---

## 🧰 Tech Stack

| Layer | Technology |
|---|---|
| **Runtime** | .NET 9.0 |
| **Framework** | ASP.NET Core Web API |
| **ORM** | Entity Framework Core 9 |
| **Database** | SQL Server |
| **Identity** | ASP.NET Core Identity |
| **Cache / Basket Store** | Redis (StackExchange.Redis) |
| **Payments** | Stripe.net |
| **Mapping** | AutoMapper |
| **Auth** | JWT Bearer Authentication |

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?style=flat-square)
![Stripe](https://img.shields.io/badge/Stripe-Payments-635BFF?style=flat-square&logo=stripe)
![Redis](https://img.shields.io/badge/Redis-Cache-DC382D?style=flat-square&logo=redis)
![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=flat-square&logo=microsoftsqlserver)

---

## 🏛️ Architecture & Design Patterns

The solution follows **Clean Architecture** with four main projects:

```
Talabat.Solution.API/
├── Talabat.API/          # Controllers, DTOs, Middleware, AutoMapper profiles
├── Talabat.Core/         # Entities, Specifications, Repository & Service contracts
├── Talabat.Services/     # Business logic (Orders, Payments, Products, Tokens)
└── Talabat.Repository/   # EF Core DbContext, Generic Repository, Unit of Work
```

### Key Patterns

| Pattern | Usage |
|---|---|
| **Repository Pattern** | `IGenericRepository<T>` abstracts data access for all entities |
| **Unit of Work** | `IUnitOfWork` coordinates transactions across repositories |
| **Specification Pattern** | Composable query specs (filtering, includes, sorting) — e.g. `OrderSpecifications` sorts by `OrderDate` descending |
| **DTO Mapping** | AutoMapper maps entities ↔ API response models |
| **Dependency Injection** | All services registered in `ApplicationServicesExtension` |

### Security Model

Authenticated endpoints resolve the buyer from the JWT token:

```csharp
var buyerEmail = User.FindFirstValue(ClaimTypes.Email);
```

Orders and payment intents are always scoped to the authenticated user. The Stripe webhook is `[AllowAnonymous]` and updates orders by `PaymentIntentId` only.

---

## 🔌 API Endpoints Overview

Base URL: `https://localhost:7099/api`

### 🔑 Account — `/api/account`

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/login` | ❌ | Authenticate and receive JWT |
| `POST` | `/register` | ❌ | Register a new user |
| `GET` | `/emailexists?email=` | ❌ | Check if email is already registered |

### 🛍️ Products — `/api/product`

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/` | ❌ | Get paginated, filterable product list |
| `GET` | `/{id}` | ❌ | Get product by ID |
| `GET` | `/brands` | ❌ | Get all product brands |
| `GET` | `/categories` | ❌ | Get all product categories |

### 🧺 Basket — `/api/basket`

| Method | Route | Auth | Description |
|---|---|---|---|
| `GET` | `/?basketid=` | ❌ | Get basket by ID (Redis) |
| `POST` | `/` | ❌ | Create or update basket |
| `DELETE` | `/?basketId=` | ❌ | Delete basket |

### 📦 Orders — `/api/orders`

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/` | ✅ | Create order (buyer email from JWT) |
| `GET` | `/` | ✅ | Get all orders for authenticated user (newest first) |
| `GET` | `/{id}` | ✅ | Get order details by ID |
| `GET` | `/deliveryMethod` | ❌ | Get available delivery methods |

### 💳 Payments — `/api/payments`

| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/{basketId}` | ✅ | Create/update Stripe PaymentIntent; registers Stripe Customer from JWT email |
| `POST` | `/webhook` | ❌ | Stripe webhook — updates order status via `PaymentIntentId` |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQL Server](https://www.microsoft.com/sql-server) (LocalDB or full instance)
- [Redis](https://redis.io/) running locally
- [Stripe](https://stripe.com/) account (test mode)
- [Stripe CLI](https://stripe.com/docs/stripe-cli) (recommended for local webhook testing)

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/talabat-api.git
cd talabat-api
```

### 2. Configure `appsettings.json`

Update `Talabat.API/appsettings.json` with your local settings:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=TalabatDatabase;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;",
    "IdentityConnection": "Server=.;Database=TalabatDatabaseIdentity;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;",
    "Redis": "localhost"
  },
  "JWT": {
    "key": "YourSuperSecretKeyAtLeast32CharactersLong!",
    "validIssure": "https://localhost:7099",
    "validAudience": "MySecretApiUser",
    "DurationInDays": 30
  },
  "ApiBaseUrl": "https://localhost:7099",
  "StripeSettings": {
    "PublishableKey": "pk_test_...",
    "SecretKey": "sk_test_...",
    "WebhookSecret": "whsec_..."
  }
}
```

> ⚠️ **Never commit real API keys.** Use [User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables in production.

### 3. Apply Database Migrations

```bash
cd Talabat.API
dotnet ef database update --project ../Talabat.Repository
```

This creates the store and identity databases and seeds initial product/delivery data.

### 4. Run the API

```bash
dotnet run --launch-profile https
```

The API will be available at:

- **HTTPS:** `https://localhost:7099`
- **HTTP:** `http://localhost:5121`

### 5. Forward Stripe Webhooks (Local Development)

In a separate terminal:

```bash
stripe listen --forward-to https://localhost:7099/api/payments/webhook
```

Copy the webhook signing secret (`whsec_...`) into `StripeSettings:WebhookSecret` in `appsettings.json`.

---

## 🔄 Payment & Order Lifecycle

```
1. Client authenticates          →  JWT issued with email claim
2. Client adds items to basket   →  Stored in Redis
3. Client starts checkout        →  POST /api/payments/{basketId}
                                   →  Stripe Customer created/found by email
                                   →  PaymentIntent linked to Customer
4. Client confirms card payment  →  Stripe processes payment
5. Order is created              →  POST /api/orders (email from JWT)
                                   →  Status set to PaymentReceived if intent succeeded
6. Stripe webhook fires          →  POST /api/payments/webhook
                                   →  Order status updated by PaymentIntentId
```

---

## 📁 Solution Structure

| Project | Responsibility |
|---|---|
| `Talabat.API` | HTTP layer, controllers, middleware, DTOs, caching attributes |
| `Talabat.Core` | Domain entities, specifications, interfaces |
| `Talabat.Services` | Order, payment, product, and token business logic |
| `Talabat.Repository` | EF Core contexts, migrations, repository implementations |

---

## 📄 License

This project is part of the **Talabat E-Commerce** portfolio. All rights reserved.

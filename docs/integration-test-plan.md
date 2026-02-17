# Integration Test Plan

## Overview

Integration tests verify that components work together through real infrastructure — a real PostgreSQL database, the real ASP.NET Core HTTP pipeline, and real EF Core behavior. Dependencies that cross network boundaries (Stripe) are mocked.

**Framework:** xUnit  
**Database:** Testcontainers (PostgreSQL in Docker)  
**HTTP Pipeline:** WebApplicationFactory  
**Mocking:** NSubstitute (for IStripeService only)  
**Project:** `ECommerce.IntegrationTests`

---

## Infrastructure

### `PostgresFixture` (collection fixture)

Shared across all test classes via `[CollectionDefinition]`. Starts a PostgreSQL container once, applies EF migrations, provides a connection string. Each test class gets a fresh `ECommerceDbContext` scoped to a transaction that rolls back after each test for isolation.

### `CustomWebApplicationFactory`

Extends `WebApplicationFactory<Program>`. Overrides:
- Connection string → Testcontainers PostgreSQL
- `IStripeService` → NSubstitute mock (returns fake checkout URLs)
- Disables TickerQ scheduler (background jobs tested separately)
- Provides helper methods: `CreateAuthenticatedClient(role, userId)` that injects a JWT into the `HttpClient`

### `TestDatabaseHelper`

Seed methods for common scenarios:
- `SeedBuyerWithCart()` → Buyer + Cart (via `Buyer.Create`)
- `SeedSellerWithProducts()` → Seller + N products
- `SeedFullCheckoutScenario()` → Buyer + Seller + Products + CartItems + Transaction + Orders

---

## P1 — StockReservationService (real DB, concurrency)

The most critical code. Tests run against real PostgreSQL to verify concurrency tokens, retry logic, and stock arithmetic.

### `ReserveStockForCartItems`

```
B1: product found, sufficient stock → ReservedCount incremented
B2: product not found → ProductNotFoundException
B3: insufficient stock → InsufficientStockException
B4: concurrency conflict on save → retry succeeds with refreshed values
```

| # | Test | Branch |
|---|------|--------|
| 1 | Single item, sufficient stock → ReservedCount increases | B1 |
| 2 | Multiple items across products → all ReservedCounts increase | B1 |
| 3 | Product ID not in database → throws ProductNotFoundException | B2 |
| 4 | Requested count > AvailableStock → throws InsufficientStockException | B3 |
| 5 | Exactly AvailableStock → succeeds (boundary) | B1 |
| 6 | Stock already partially reserved, new request exceeds remaining → throws | B3 |
| 7 | Concurrent modification of same product → retry succeeds | B4 |

### `ConfirmReservation`

```
B1: transaction found, status Processing → stock subtracted, orders InTransit, status Success
B2: transaction null → no-op return
B3: transaction exists but status != Processing → no-op return
```

| # | Test | Branch |
|---|------|--------|
| 8 | Processing transaction → CountInStock decreases, ReservedCount decreases, orders InTransit | B1 |
| 9 | Transaction status = Success (already confirmed) → no changes | B3 |
| 10 | Non-existent transaction ID → no changes | B2 |
| 11 | Multiple orders on same transaction → all updated atomically | B1 |
| 12 | Transaction.Status set to Success after confirm | B1 |

### `ReleaseReservation`

```
B1: transaction found, status Processing → ReservedCount decreases, orders Cancelled, status Expired
B2: transaction null → no-op (null?.Status != Processing is true)
B3: transaction exists but status != Processing → no-op
```

| # | Test | Branch |
|---|------|--------|
| 13 | Processing transaction → ReservedCount decreases, orders Cancelled | B1 |
| 14 | Transaction.Status set to Expired after release | B1 |
| 15 | Already expired transaction → no changes | B3 |
| 16 | Non-existent transaction ID → no changes | B2 |
| 17 | CountInStock unchanged after release (only ReservedCount changes) | B1 |

---

## P2 — Database Constraints & Exception Translation

Verify that PostgreSQL check constraints and unique indexes work, and that `SaveChangesAsync` translates them correctly.

### Check Constraints

| # | Test | Constraint |
|---|------|------------|
| 18 | Product with Price = 0 → DB rejects | CK_Product_Price_Positive |
| 19 | Product with negative CountInStock → DB rejects | CK_Product_Stock_Positive |
| 20 | Product with negative ReservedCount → DB rejects | CK_Product_Reserved_Non_Negative |
| 21 | Product with ReservedCount > CountInStock → DB rejects | CK_Product_Reserved_Within_Stock |
| 22 | Order with Count = 0 → DB rejects | CK_Ordered_Product_Count_Positive |
| 23 | Order with negative Total → DB rejects | CK_Order_Total_Positive |
| 24 | CartItem with Count = 0 → DB rejects | CK_CartItem_Count_Positive |

### Unique Index Exception Translation

| # | Test | Branch |
|---|------|--------|
| 25 | Duplicate buyer email → throws DuplicateEmailException | IX_Buyers_Email |
| 26 | Duplicate seller email → throws DuplicateEmailException | IX_Sellers_Email |
| 27 | Duplicate SKU + SellerId → throws DuplicateSkuException | IX_Products_Sku_SellerId |
| 28 | Same SKU, different seller → succeeds | Composite key allows it |
| 29 | Unknown unique constraint violation → rethrows original DbUpdateException | Default arm |

---

## P3 — Repository Tests (real DB)

### `AuthRepository`

```
GetBuyerIfValidCredentialsAsync:
  B1: email found, password matches → return buyer
  B2: email found, password wrong → return null
  B3: email not found → return null
GetSellerIfValidCredentialsAsync: same pattern
```

| # | Test | Branch |
|---|------|--------|
| 30 | Valid buyer credentials → returns buyer | B1 |
| 31 | Wrong buyer password → returns null | B2 |
| 32 | Non-existent buyer email → returns null | B3 |
| 33 | Valid seller credentials → returns seller | B1 |
| 34 | Wrong seller password → returns null | B2 |
| 35 | Non-existent seller email → returns null | B3 |
| 36 | GetBuyerByIdAsync returns correct buyer | Direct lookup |

### `CartRepository`

```
AddOrUpdateCartAsync:
  B1: item doesn't exist → creates new CartItem
  B2: item exists → updates Count
DeleteProductFromCartAsync:
  B1: item exists → removes it
  B2: item doesn't exist → no-op
```

| # | Test | Branch |
|---|------|--------|
| 37 | Add new item to cart → CartItem created | B1 |
| 38 | Update existing item count → Count changes | B2 |
| 39 | Get cart items → returns items with Product included | Include verification |
| 40 | Delete existing item → removed | B1 |
| 41 | Delete non-existent item → no error | B2 |
| 42 | Clear cart → all items removed | ExecuteDeleteAsync |
| 43 | Get empty cart → returns empty list | Empty case |

### `ProductsRepository`

```
GetProductsBySellerIdAsync:
  B1: sellerIds filter applied
  B2: productIds filter applied (when not null and count > 0)
GetAllListedProductsAsync: filters by IsListed = true
GetListedProductsByIdAsync:
  B1: product exists and IsListed → returns product
  B2: product exists but not listed → returns null
  B3: product doesn't exist → returns null
```

| # | Test | Branch |
|---|------|--------|
| 44 | Get by seller → only returns that seller's products | B1 |
| 45 | Get by seller + product ID → returns single match | B1 + B2 |
| 46 | Get by seller, product belongs to other seller → empty | Scoping |
| 47 | Get all listed → excludes unlisted products | IsListed filter |
| 48 | Get listed by ID, product is listed → returns product | B1 |
| 49 | Get listed by ID, product is unlisted → returns null | B2 |
| 50 | Get listed by ID, product doesn't exist → returns null | B3 |

### `OrdersRepository`

```
GetOrdersAsync:
  B1: Seller scope → only seller's orders
  B2: Buyer scope → only buyer's orders
  B3: orderIds filter (when not null and count > 0)
  B4: productIds filter (when not null and count > 0)
```

| # | Test | Branch |
|---|------|--------|
| 51 | Buyer scope → only returns that buyer's orders | B2 |
| 52 | Seller scope → only returns that seller's orders | B1 |
| 53 | Buyer can't see other buyer's orders | Authorization |
| 54 | Seller can't see other seller's orders | Authorization |
| 55 | Filter by orderId → returns single match | B3 |
| 56 | Filter by productId → returns matching orders | B4 |
| 57 | Orders include Product navigation property | Include verification |

### `TransactionRepository`

| # | Test | Branch |
|---|------|--------|
| 58 | CreateTransactionForCartItems computes correct amount | Arithmetic |
| 59 | CreateTransactionForCartItems sets status Processing | Initial state |
| 60 | GetByStripeSessionIdAsync finds matching transaction | Found |
| 61 | GetByStripeSessionIdAsync returns null for unknown session | Not found |

---

## P4 — Cleanup Job SQL (real DB)

The UUIDv7 timestamp extraction SQL must work against real PostgreSQL.

| # | Test | Branch |
|---|------|--------|
| 62 | Stale Processing transaction (>15 min old UUIDv7) → found by query | Filter matches |
| 63 | Recent Processing transaction (<15 min old) → NOT found | Filter excludes |
| 64 | Stale but already Expired transaction → NOT found | Status filter |
| 65 | Stale but already Success transaction → NOT found | Status filter |
| 66 | No stale transactions → returns empty, no ReleaseReservation calls | Early return |

---

## P5 — HTTP Pipeline (WebApplicationFactory)

Full request → middleware → filters → controller → DB → response. Tests the real pipeline with a real database, mocked Stripe only.

### Authentication & Authorization

| # | Test | Endpoint |
|---|------|----------|
| 67 | Unauthenticated request to buyer endpoint → 401 | GET /buyer/products |
| 68 | Unauthenticated request to seller endpoint → 401 | GET /seller/products |
| 69 | Buyer token on seller endpoint → 403 | GET /seller/products |
| 70 | Seller token on buyer endpoint → 403 | GET /buyer/cart |
| 71 | Auth endpoints accessible without token → 200 or 401 | POST /auth/buyer/login |

### Auth Controller

| # | Test | Branch |
|---|------|--------|
| 72 | Register buyer → 200, buyer exists in DB | Happy path |
| 73 | Register buyer with duplicate email → 409 DuplicateEmailException | Unique violation |
| 74 | Register buyer with invalid body → 400 ValidationProblemDetails | FluentValidation |
| 75 | Register seller → 200, seller exists in DB | Happy path |
| 76 | Register seller with duplicate email → 409 | Unique violation |
| 77 | Login buyer with valid credentials → 200 + accessToken | B1 |
| 78 | Login buyer with wrong password → 401 | B2 |
| 79 | Login buyer with non-existent email → 401 | B3 |
| 80 | Login seller with valid credentials → 200 + accessToken | B1 |

### Buyer Products Controller

| # | Test | Branch |
|---|------|--------|
| 81 | Get all products → returns only listed products | Filter |
| 82 | Get product by ID (listed) → 200 + DTO | B1 |
| 83 | Get product by ID (unlisted) → 404 | B2 |
| 84 | Get product by ID (non-existent) → 404 | B3 |

### Buyer Cart Controller

| # | Test | Branch |
|---|------|--------|
| 85 | Add item to cart → 200, item in DB | Add path |
| 86 | Update item count in cart → 200, count updated | Update path |
| 87 | Add with count ≤ 0 → 400 | Validation |
| 88 | Get cart → returns items with product data | Happy path |
| 89 | Delete item from cart → 204 | Delete path |
| 90 | Clear cart → 204, all items removed | Clear path |
| 91 | Place orders with items → 200 + checkoutUrl | Happy path |
| 92 | Place orders with empty cart → 400 | Empty cart |
| 93 | Place orders with insufficient stock → 422 InsufficientStockException | Stock check |

### Seller Products Controller

| # | Test | Branch |
|---|------|--------|
| 94 | Get all seller products → returns only that seller's products | Scoping |
| 95 | Get product by ID → 200 | Happy path |
| 96 | Get product belonging to other seller → 404 | Scoping |
| 97 | Add product → 201, product in DB | Create |
| 98 | Add product with duplicate SKU → 409 | Unique violation |
| 99 | Update product (PATCH) → 200, fields updated | Partial update |
| 100 | Update non-existent product → 404 | Not found |
| 101 | Set stock → 200, CountInStock updated | Happy path |
| 102 | Set stock below reserved → 422 StockBelowReservedException | Validation |
| 103 | Set stock on non-existent product → 404 | Not found |

### Seller Orders Controller

| # | Test | Branch |
|---|------|--------|
| 104 | Get all seller orders → returns only that seller's orders | Scoping |
| 105 | Get order by ID → 200 | Happy path |
| 106 | Get order belonging to other seller → 404 | Scoping |
| 107 | Update order InTransit → Delivered → 200 | Valid transition |
| 108 | Update order with invalid transition → 422 | Invalid transition |
| 109 | Update non-existent order → 404 | Not found |

### DbTransactionFilter (through pipeline)

| # | Test | Branch |
|---|------|--------|
| 110 | Successful POST → data committed to DB | Commit path |
| 111 | Failed validation → data NOT committed (rollback) | Rollback on 4xx |
| 112 | Domain exception → data NOT committed (rollback) | Rollback on exception |
| 113 | GET request → no transaction overhead (data readable) | GET skip |

---

## P6 — Checkout & Payment Flow (end-to-end)

Full flow: add to cart → place order → webhook confirm/expire.

| # | Test | Scenario |
|---|------|----------|
| 114 | Full happy path: register → login → add to cart → place order → confirm webhook → stock subtracted, orders InTransit | Complete flow |
| 115 | Place order → expire webhook → stock released, orders Cancelled | Expiry flow |
| 116 | Place order → confirm webhook → confirm again (idempotent, no double subtraction) | Duplicate webhook |
| 117 | Place order → expire webhook → expire again (idempotent) | Duplicate webhook |
| 118 | Place order for multiple products → all reserved, all confirmed | Multi-product |

---

## Project Structure

```
ECommerce.IntegrationTests/
├── Fixtures/
│   ├── PostgresFixture.cs
│   ├── CustomWebApplicationFactory.cs
│   └── TestDatabaseHelper.cs
├── Helpers/
│   └── HttpClientExtensions.cs      (auth header helpers)
├── StockReservation/
│   └── StockReservationServiceTests.cs
├── Database/
│   ├── CheckConstraintTests.cs
│   └── ExceptionTranslationTests.cs
├── Repositories/
│   ├── AuthRepositoryTests.cs
│   ├── CartRepositoryTests.cs
│   ├── ProductsRepositoryTests.cs
│   ├── OrdersRepositoryTests.cs
│   └── TransactionRepositoryTests.cs
├── CleanupJob/
│   └── ReservationCleanupQueryTests.cs
├── Pipeline/
│   ├── AuthEndpointTests.cs
│   ├── BuyerProductEndpointTests.cs
│   ├── BuyerCartEndpointTests.cs
│   ├── SellerProductEndpointTests.cs
│   ├── SellerOrderEndpointTests.cs
│   ├── AuthorizationTests.cs
│   └── TransactionFilterTests.cs
├── Flows/
│   └── CheckoutFlowTests.cs
```

---

## Conventions

- **Collection fixture:** All test classes share a single PostgreSQL container via `[Collection("Database")]`
- **Test isolation:** Each test method gets a fresh DB state — either via transaction rollback or by seeding unique data with `Guid.NewGuid()` prefixes
- **Naming:** `MethodName_Scenario_ExpectedResult`
- **Authenticated requests:** Helper creates JWT with correct claims, injects via `Authorization` header
- **Stripe mock:** NSubstitute `IStripeService` returns `"https://fake-checkout.stripe.com"` for all calls
- **No test interdependence:** Each test seeds its own data

---

## Total: 118 integration tests across 6 categories

| Category | Tests | What it verifies |
|----------|-------|------------------|
| P1 — StockReservationService | 1–17 | Concurrency retry, stock arithmetic, idempotency |
| P2 — DB Constraints & Translation | 18–29 | Check constraints, unique indexes, exception mapping |
| P3 — Repositories | 30–61 | CRUD correctness, query scoping, authorization boundaries |
| P4 — Cleanup Job SQL | 62–66 | UUIDv7 timestamp extraction in real PostgreSQL |
| P5 — HTTP Pipeline | 67–113 | Auth, filters, controllers, commit/rollback, full request cycle |
| P6 — Checkout Flows | 114–118 | End-to-end multi-step payment scenarios |

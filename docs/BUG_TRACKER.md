# eCommerce Bug Tracker & Architecture Decisions

## Architectural Decisions Made

### 1. Unit of Work — Controllers Own Persistence

**Problem:** Repositories were calling `SaveChangesAsync` internally, creating ambiguity about who owns the unit of work. The `DbTransactionFilter` wraps actions in a DB transaction, but repositories behaved as if they were autonomous — resulting in multiple savepoints, confusing ownership, and the `ChangeTracker.Clear()` bug in the retry path.

**Decision:** Repositories are now pure change-tracker mutators. They never call `SaveChangesAsync`. Controllers call `unitOfWork.SaveChangesAsync()` when all mutations are staged. `StockReservationService` is the sole exception — it retains its own `SaveChangesAsync` because concurrency token validation requires a flush.

**Changes:**
- All repository `Create`/`Add`/`Update` methods changed from `async Task` to `void` (or synchronous equivalents)
- Controllers inject `IUnitOfWork` and call `SaveChangesAsync` explicitly
- `IUnitOfWork` registered in DI: `builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ECommerceDbContext>())`

### 2. Exception Translation — DbContext SaveChangesAsync Override

**Problem:** Repositories were catching `DbUpdateException` with Postgres constraint names and translating them to domain exceptions. This forced repositories to call `SaveChangesAsync` internally (to trigger the constraint check) and duplicated catch logic across multiple repositories.

**Decision:** Centralized all constraint-violation-to-domain-exception translation in a `SaveChangesAsync` override on `ECommerceDbContext`. This is the Exception Shielding / Fault Barrier pattern — infrastructure exceptions are translated at the infrastructure boundary before they cross into higher layers.

**Pipeline:** Postgres constraint violation → `SaveChangesAsync` override → domain exception → `DomainExceptionHandler` → HTTP response

**Changes:**
- `ECommerceDbContext.SaveChangesAsync` override catches `DbUpdateException` with `PostgresException.UniqueViolation` and maps constraint names to domain exceptions via a switch expression
- All try/catch blocks removed from repositories
- Repositories no longer reference `Npgsql` or `PostgresException`

### 3. Concurrency Retry — Targeted Product Refresh

**Problem:** `StockReservationService.ExecuteWithRetry` called `dbContext.ChangeTracker.Clear()` on `DbUpdateConcurrencyException`, which detached all tracked entities — including cart items, buyer, and other entities from the outer `PlaceOrders` flow. Subsequent operations on those detached entities would fail or produce stale data.

**Decision:** Replaced `ChangeTracker.Clear()` with `RefreshConflictingProducts` — a method that queries only the failed `Product` entities in a single round trip using `AsNoTracking`, then updates `CurrentValues` and `OriginalValues` on the tracked entries. This resets them to a clean "just loaded" state so the retry lambda can re-apply mutations with correct concurrency tokens, while preserving all other tracked entities.

**Why both CurrentValues and OriginalValues are set to the same fresh values:** This makes the entity look unmodified ("clean") to EF. The retry lambda then re-applies the mutations (e.g., `product.ReservedCount += ci.Count`), creating the correct dirty state for the next `SaveChangesAsync`.

### 4. StripeService Decoupled from DbContext

**Problem:** `StripeService` had a direct dependency on `ECommerceDbContext` and called `SaveChangesAsync` internally after setting `transaction.StripeSessionId`. This violated the "controllers own persistence" rule and coupled a Stripe integration to EF.

**Decision:** Removed the `ECommerceDbContext` dependency from `StripeService`. It now sets `transaction.StripeSessionId` on the tracked entity and returns. The controller flushes the change.

---

## Bugs Found — Status

### Bug 1: `PlaceOrders` — `ChangeTracker.Clear()` detaches outer entities ✅ FIXED
**Severity:** Critical
**Description:** `StockReservationService.ExecuteWithRetry` called `ChangeTracker.Clear()` on concurrency failure, detaching all tracked entities including cart items loaded by the controller. After retry, subsequent repository calls operated on detached entities.
**Fix:** Replaced `ChangeTracker.Clear()` with `RefreshConflictingProducts()` that refreshes only the conflicting Product rows in a single query.

### Bug 2: `ReleaseReservation` double-decrement on race condition ✅ FIXED
**Severity:** Critical
**Description:** Guard clause `if (transaction.Status == TransactionStatus.Success) return;` allowed release on already-`Expired` transactions. If cleanup job and webhook both called `ReleaseReservation`, the second call would double-decrement `ReservedCount`, violating `CK_Product_Reserved_Non_Negative`.
**Fix:** Changed guard to `if (transaction.Status != TransactionStatus.Processing) return;` — only release if still in `Processing` state.

### Bug 3: `ConfirmReservation` double-execution risk ✅ FIXED (by Bug 1 fix)
**Severity:** Medium
**Description:** Concurrent webhook retries could both read `Processing` and proceed. Concurrency tokens would cause one to fail, and the retry with `ChangeTracker.Clear()` would re-read `Success` and return early — accidentally correct but fragile.
**Fix:** The targeted refresh approach makes this explicit and documented. Concurrency tokens still protect against double-execution, but the retry mechanism is now clean rather than accidentally correct.

### Bug 4: `CartItem.Count` has no check constraint ⬜ TODO
**Severity:** Low
**Description:** `CartItem.Count` initializes to 0. No DB-level check constraint prevents `Count <= 0`. The `Order` table has `CK_Ordered_Product_Count_Positive` but `CartItem` does not.
**Fix:** Add check constraint `"Count" > 0` on CartItem table.

### Bug 5: `AddOrUpdateCart` doesn't validate available stock ⬜ TODO
**Severity:** Low (design choice)
**Description:** A buyer can add unlimited items to cart regardless of available stock. Stock check only happens at `PlaceOrders` time. Poor UX — buyer discovers insufficient stock only at checkout.
**Fix:** Consider validating against `AvailableStock` in `AddOrUpdateCartAsync`, or at minimum show a warning on the frontend.

### Bug 6: `BuyerPaymentsController` is empty dead code ⬜ TODO
**Severity:** Low
**Description:** Empty controller with `DbContext` injection, wrong route convention (`api/[controller]` vs `buyer/payments`), shows up in Swagger.
**Fix:** Delete the file.

### Bug 7: `ProductsRepository` interface vs implementation mismatch ⬜ TODO
**Severity:** Medium
**Description:** Interface declares `sellerIds` as non-nullable `IReadOnlyCollection<Guid>`. Implementation declares it as nullable with default null. If called with null, returns all products across all sellers — authorization bypass.
**Fix:** Make implementation match interface: non-nullable `sellerIds`, always filter.

### Bug 8: Multiple `SaveChangesAsync` calls within `DbTransactionFilter` scope ✅ FIXED
**Severity:** Medium
**Description:** Repositories called `SaveChangesAsync` internally while also being wrapped in a DB transaction filter, creating savepoints and confusing ownership.
**Fix:** Repositories no longer call `SaveChangesAsync`. Controllers own persistence via `IUnitOfWork`.

### Bug 9: `ReservationCleanupJob` loads all processing transactions into memory ⬜ TODO
**Severity:** Medium
**Description:** Loads every `Processing` transaction into memory every 5 minutes, then filters in-memory by `CreatedAt` (a computed property from UUIDv7, not a DB column). Won't scale.
**Fix:** Add a persisted `CreatedAt` column, or use raw SQL to extract timestamp from UUIDv7 at the database level.

### Bug 10: Missing startup validation for required config ⬜ TODO
**Severity:** P2
**Description:** `appsettings.json` contains a local dev connection string (`localhost`, `Password=admin`) — not a real secret. However, `JWT:SecretKey`, `Stripe:SecretKey`, and `Stripe:WebhookSecret` are expected in user secrets with no startup validation. If someone clones the repo and runs without configuring user secrets, `configuration["JWT:SecretKey"]` returns null, `Encoding.UTF8.GetBytes(null)` throws `ArgumentNullException` — a cryptic crash instead of a clear message.
**Fix:** Add startup validation that fails fast with a descriptive error if required config values are missing.

### Bug 11: CORS `AllowAll` with `AllowAnyOrigin` ⬜ TODO
**Severity:** Medium
**Description:** Wide-open CORS policy. Fine for dev, not for production.
**Fix:** Restrict to known frontend origins in production.

### Bug 12: No input validation on registration DTOs ✅ FIXED
**Severity:** High
**Description:** No length limits, email format validation, or password strength requirements. A user could register with a 10MB string as their name.
**Fix:** Added FluentValidation with auto-validation. Validators created for all request DTOs: `BuyerRegisterRequestValidator`, `SellerRegisterRequestValidator`, `LoginRequestValidator`, `AddProductValidator`, `UpdateProductValidator`, `SetStockValidator`, `UpdateOrderValidator`. Validation runs automatically in the MVC pipeline before the controller action executes — invalid requests return 400 with structured error details.

### Bug 13: `BankAccountNumber` stored in plain text ⬜ TODO
**Severity:** Medium
**Description:** Sensitive financial PII stored unencrypted.
**Fix:** Encrypt at rest or tokenize.

### Bug 14: `DbTransactionFilter` + `StripeWebhookController` interaction ⬜ TODO
**Severity:** Low
**Description:** Webhook is wrapped in the global `DbTransactionFilter`. The reservation service has its own retry loop. Two layers of error handling that can interact unpredictably.
**Fix:** Consider excluding the webhook controller from the filter, or accept the current behavior with documentation.

### Bug 15: `StripeService` creates new `StripeClient` per request ⬜ TODO
**Severity:** Medium
**Description:** `StripeClient` created in constructor of a scoped service. New HTTP client per request can cause socket exhaustion under load.
**Fix:** Register `StripeClient` as singleton or use `IHttpClientFactory`.

### Bug 16: AutoMapper `ForAllMembers` null condition on `UpdateProductDto` ⬜ TODO
**Severity:** Low
**Description:** PATCH cannot set nullable fields to null. `{"description": null}` is silently ignored by AutoMapper.
**Fix:** Use a different pattern for partial updates (e.g., `JsonPatchDocument`, or explicit null-vs-absent handling).

### Bug 17: No auth guard on frontend buyer routes ⬜ TODO
**Severity:** Medium
**Description:** Unauthenticated users can navigate to `/buyer`. API returns 401 but no redirect to login. `AuthContext` is commented out.
**Fix:** Implement auth guard that checks for token and redirects to login.

### Bug 18: `view_orders.tsx` references non-existent `Accepted` status ⬜ TODO
**Severity:** Low
**Description:** Frontend `statusConfig` includes `Accepted` which doesn't exist in backend `OrderStatus` enum. Dead code, but signals frontend/backend desync.
**Fix:** Remove `Accepted` from status config.

### Bug 19: `place_order.tsx` uses `window.location.href` ⬜ INFO
**Severity:** Info
**Description:** Hard navigation to Stripe checkout breaks SPA state. Full page load on return from Stripe. Acceptable for this flow but noted.

### Bug 20: Logout doesn't invalidate JWT ⬜ TODO
**Severity:** Medium
**Description:** Token removed from `localStorage` but remains valid for its 1-day lifetime.
**Fix:** Shorten JWT expiry, implement refresh tokens, or add server-side token blocklist.

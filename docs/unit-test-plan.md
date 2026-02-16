# Unit Test Plan

## Overview

This document outlines the unit test strategy for the eCommerce backend. Unit tests verify individual classes and methods in isolation — dependencies are replaced with NSubstitute mocks. No database, no network, no Docker.

**Framework:** xUnit  
**Mocking:** NSubstitute  
**Project:** `ECommerce.Tests` (separate project referencing `ECommerce`)

Each test maps to a specific code branch. Branch references (e.g. `[B1]`) are noted so coverage can be audited.

---

## P1 — Domain Entity Logic

Pure logic, zero dependencies.

### `Order.TransitionTo`

The conditional has two independent branch points:

```csharp
if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
```

- **B1:** `TryGetValue` fails — Status has no entry in `ValidTransitions` → throw
- **B2:** `TryGetValue` succeeds but `newStatus` not in allowed set → throw
- **B3:** Both pass → success

| # | Test | Branch | Input |
|---|------|--------|-------|
| 1 | InTransit → Delivered succeeds | B3 | Only valid public transition |
| 2 | AwaitingPayment → Delivered throws | B1 | Status not in dict |
| 3 | Cancelled → InTransit throws | B1 | Terminal state, not in dict |
| 4 | Delivered → InTransit throws | B1 | Terminal state, not in dict |
| 5 | AwaitingPayment → InTransit throws | B1 | Only system can do this |
| 6 | InTransit → Cancelled throws | B2 | Status in dict, but target not in allowed set |
| 7 | InTransit → AwaitingPayment throws | B2 | Status in dict, but target not in allowed set |

### `Order.MarkInTransit` / `Order.MarkCancelled`

No branches — direct assignment. Require `[InternalsVisibleTo("ECommerce.Tests")]`.

| # | Test | Why |
|---|------|-----|
| 8 | MarkInTransit sets Status to InTransit | System transition |
| 9 | MarkCancelled sets Status to Cancelled | System transition |

### `Order.Create`

No branches — property assignments. Test each output field.

| # | Test | Field verified |
|---|------|----------------|
| 10 | Sets BuyerId from buyer.Id | FK correctness |
| 11 | Sets SellerId from cartItem.Product.SellerId | FK correctness |
| 12 | Sets ProductId from cartItem.ProductId | FK correctness |
| 13 | Computes Total as Product.Price × CartItem.Count | Arithmetic |
| 14 | Sets Address from buyer.Address | Data propagation |
| 15 | Sets Status to AwaitingPayment | Initial state |
| 16 | Sets Transaction reference | FK to transaction |
| 17 | Sets Count from cartItem.Count | Data propagation |

### `Buyer.Create`

No branches.

| # | Test | Why |
|---|------|-----|
| 18 | Sets Name, Email, PasswordHash, Address | All fields propagated |
| 19 | Cart is not null | Cart auto-creation invariant |
| 20 | Cart.Buyer references the created buyer | Bidirectional link |

### `Product.AvailableStock`

No branches — single expression `CountInStock - ReservedCount`.

| # | Test | Input | Expected |
|---|------|-------|----------|
| 21 | Normal stock | Stock=10, Reserved=3 | 7 |
| 22 | Fully reserved | Stock=5, Reserved=5 | 0 |
| 23 | No reservations | Stock=10, Reserved=0 | 10 |

### `Transaction.CreatedAt`

No branches — bit manipulation on UUIDv7.

| # | Test | Why |
|---|------|-----|
| 24 | Extracts correct timestamp from known UUIDv7 | Verify hex→ms→DateTime logic |

---

## P2 — FluentValidation Validators

Each rule creates branches: the value passes or fails each constraint. Every `.When()` guard creates an additional branch (condition true → validate, condition false → skip).

### `BuyerRegisterRequestValidator`

Rules: Name(NotEmpty, MaxLen=100), Email(NotEmpty, EmailAddress, MaxLen=256), Password(NotEmpty, MinLen=8, MaxLen=128), Address(NotEmpty, MaxLen=500)

| # | Test | Field | Branch |
|---|------|-------|--------|
| 25 | Valid input passes | All | All pass |
| 26 | Empty name fails | Name | NotEmpty |
| 27 | Name > 100 chars fails | Name | MaximumLength |
| 28 | Empty email fails | Email | NotEmpty |
| 29 | Invalid email format fails | Email | EmailAddress |
| 30 | Email > 256 chars fails | Email | MaximumLength |
| 31 | Empty password fails | Password | NotEmpty |
| 32 | Password < 8 chars fails | Password | MinimumLength |
| 33 | Password = 8 chars passes | Password | MinimumLength boundary |
| 34 | Password > 128 chars fails | Password | MaximumLength |
| 35 | Empty address fails | Address | NotEmpty |
| 36 | Address > 500 chars fails | Address | MaximumLength |

### `SellerRegisterRequestValidator`

Rules: Name(NotEmpty, MaxLen=100), Email(NotEmpty, EmailAddress, MaxLen=256), Password(NotEmpty, MinLen=8, MaxLen=128), BankAccountNumber(NotEmpty, MaxLen=34)

| # | Test | Field | Branch |
|---|------|-------|--------|
| 37 | Valid input passes | All | All pass |
| 38 | Empty name fails | Name | NotEmpty |
| 39 | Name > 100 chars fails | Name | MaximumLength |
| 40 | Empty email fails | Email | NotEmpty |
| 41 | Invalid email format fails | Email | EmailAddress |
| 42 | Email > 256 chars fails | Email | MaximumLength |
| 43 | Empty password fails | Password | NotEmpty |
| 44 | Password < 8 chars fails | Password | MinimumLength |
| 45 | Password > 128 chars fails | Password | MaximumLength |
| 46 | Empty bank account fails | BankAccountNumber | NotEmpty |
| 47 | Bank account > 34 chars fails | BankAccountNumber | MaximumLength |

### `LoginRequestValidator`

Rules: Email(NotEmpty, EmailAddress, MaxLen=256), Password(NotEmpty, MaxLen=128)

| # | Test | Field | Branch |
|---|------|-------|--------|
| 48 | Valid input passes | All | All pass |
| 49 | Empty email fails | Email | NotEmpty |
| 50 | Invalid email format fails | Email | EmailAddress |
| 51 | Email > 256 chars fails | Email | MaximumLength |
| 52 | Empty password fails | Password | NotEmpty |
| 53 | Password > 128 chars fails | Password | MaximumLength |

### `AddProductValidator`

Rules: Sku(NotEmpty, MaxLen=50), Name(NotEmpty, MaxLen=200), Price(>0), CountInStock(>=0), Description(MaxLen=2000 when not null), ImageUrl(MaxLen=2048, Must(Uri) when not null)

| # | Test | Field | Branch |
|---|------|-------|--------|
| 54 | Valid input with all fields passes | All | All pass |
| 55 | Empty SKU fails | Sku | NotEmpty |
| 56 | SKU > 50 chars fails | Sku | MaximumLength |
| 57 | Empty name fails | Name | NotEmpty |
| 58 | Name > 200 chars fails | Name | MaximumLength |
| 59 | Price = 0 fails | Price | GreaterThan(0) |
| 60 | Negative price fails | Price | GreaterThan(0) |
| 61 | CountInStock = 0 passes | CountInStock | GreaterThanOrEqualTo(0) boundary |
| 62 | Negative CountInStock fails | CountInStock | GreaterThanOrEqualTo(0) |
| 63 | Null Description passes | Description | When(not null) → skip |
| 64 | Description > 2000 chars fails | Description | When(not null) → validate → MaxLen |
| 65 | Valid Description passes | Description | When(not null) → validate → pass |
| 66 | Null ImageUrl passes | ImageUrl | When(not null) → skip |
| 67 | Invalid ImageUrl fails | ImageUrl | When(not null) → Must(Uri) |
| 68 | ImageUrl > 2048 chars fails | ImageUrl | When(not null) → MaxLen |
| 69 | Valid ImageUrl passes | ImageUrl | When(not null) → validate → pass |

### `UpdateProductValidator`

Rules: All fields nullable with `.When(x => x.Field is not null)` guard.
Sku(MinLen=1, MaxLen=50), Name(MinLen=1, MaxLen=200), Price(>0), Description(MaxLen=2000), ImageUrl(MaxLen=2048, Must(Uri))

| # | Test | Field | Branch |
|---|------|-------|--------|
| 70 | All nulls passes (no-op update) | All | All When guards → skip |
| 71 | Empty string SKU fails | Sku | When(not null) → MinLen(1) |
| 72 | SKU > 50 chars fails | Sku | When(not null) → MaxLen |
| 73 | Valid SKU passes | Sku | When(not null) → pass |
| 74 | Empty string Name fails | Name | When(not null) → MinLen(1) |
| 75 | Name > 200 chars fails | Name | When(not null) → MaxLen |
| 76 | Valid Name passes | Name | When(not null) → pass |
| 77 | Price = 0 fails | Price | When(not null) → GreaterThan(0) |
| 78 | Negative price fails | Price | When(not null) → GreaterThan(0) |
| 79 | Valid price passes | Price | When(not null) → pass |
| 80 | Description > 2000 chars fails | Description | When(not null) → MaxLen |
| 81 | Valid Description passes | Description | When(not null) → pass |
| 82 | Invalid ImageUrl fails | ImageUrl | When(not null) → Must(Uri) |
| 83 | ImageUrl > 2048 chars fails | ImageUrl | When(not null) → MaxLen |
| 84 | Valid ImageUrl passes | ImageUrl | When(not null) → pass |

### `SetStockValidator`

Rules: CountInStock(>=0)

| # | Test | Field | Branch |
|---|------|-------|--------|
| 85 | Zero stock passes | CountInStock | Boundary |
| 86 | Positive stock passes | CountInStock | Pass |
| 87 | Negative stock fails | CountInStock | Fail |

### `UpdateOrderValidator`

Rules: Status(IsInEnum)

| # | Test | Field | Branch |
|---|------|-------|--------|
| 88 | Valid enum value passes | Status | Pass |
| 89 | Invalid enum value fails | Status | Fail |

---

## P3 — Mapper Null Guards & Field Correctness

### `ECommerceMapper.ToBuyerOrderDto`

```csharp
ArgumentNullException.ThrowIfNull(order.Product, ...);  // B1: null → throw
return MapToBuyerOrderDto(order);                        // B2: not null → map
```

| # | Test | Branch |
|---|------|--------|
| 90 | Throws ArgumentNullException when Order.Product is null | B1 |
| 91 | Maps all fields when Order.Product is populated | B2 |
| 92 | Order.Id → OrderId | Field rename |
| 93 | Order.Total → OrderValue | Field rename |
| 94 | Order.Count → ProductCount | Field rename |
| 95 | Order.Address → DeliveryAddress | Field rename |
| 96 | Order.Status → OrderStatus | Field rename |
| 97 | Order.Product.Name → ProductName | Nested property |
| 98 | Order.Product.Sku → ProductSku | Nested property |

### `ECommerceMapper.ToSellerOrderDto`

| # | Test | Branch |
|---|------|--------|
| 99 | Throws ArgumentNullException when Order.Product is null | B1 |
| 100 | Maps all fields when Order.Product is populated | B2 |
| 101 | Does NOT map Order.Status (seller DTO has no status) | Intentional omission |

### `ECommerceMapper` — product mappings

| # | Test | Why |
|---|------|-----|
| 102 | ToBuyerProductDto maps Id, Sku, Name, Price, AvailableStock | Key fields |
| 103 | ToBuyerProductDto maps nullable Description and ImageUrl | Nullable handling |
| 104 | ToSellerProductDto maps CountInStock and IsListed | Seller-specific fields |
| 105 | ToBuyerCartItemDto does NOT set CountInCart (stays 0) | Ignored target |
| 106 | ToBuyerCartItemDto maps same fields as BuyerProductDto | Shared fields |

### `ECommerceMapper.ToProduct` (from AddProductDto)

| # | Test | Why |
|---|------|-----|
| 107 | Maps Sku, Name, Price, CountInStock, Description, ImageUrl, IsListed | All DTO fields |
| 108 | Id stays default (Guid.Empty) | Ignored target |
| 109 | SellerId stays default (Guid.Empty) | Ignored target |
| 110 | ReservedCount stays 0 | Ignored target |

### `ProductUpdateMapper.ApplyUpdate`

Mapperly generates a null check per field:
```csharp
if (source.Sku != null) target.Sku = source.Sku;  // B1: non-null → apply
                                                    // B2: null → skip
```

| # | Test | Branch |
|---|------|--------|
| 111 | Non-null Sku applied, null Name unchanged | Per-field null branching |
| 112 | Non-null Price applied, null Description unchanged | Per-field null branching |
| 113 | All non-null fields applied | All B1 paths |
| 114 | All null fields leave target unchanged | All B2 paths |
| 115 | IsListed (bool?) non-null applied | Value type nullable branch |

---

## P4 — Exception Handlers

### `DomainExceptionHandler`

```csharp
if (exception is not DomainException domainException)  // B1: not domain → false
    return false;
// B2: domain → write response + return true
```

| # | Test | Branch | Input |
|---|------|--------|-------|
| 116 | Returns false for NullReferenceException | B1 | Non-domain exception |
| 117 | Returns false for InvalidOperationException | B1 | Non-domain exception |
| 118 | Returns true for DuplicateEmailException (409) | B2 | Status = 409 |
| 119 | Returns true for InsufficientStockException (422) | B2 | Status = 422 |
| 120 | Returns true for ProductNotFoundException (404) | B2 | Status = 404 |
| 121 | Response body contains ProblemDetails with correct Title and Detail | B2 | Body verification |

### `UnhandledExceptionHandler`

No branches — always handles.

| # | Test | Why |
|---|------|-----|
| 122 | Returns true for any exception | Always catches |
| 123 | Sets status code to 500 | Correct status |
| 124 | Response body says "An unexpected error occurred" | Generic message |
| 125 | Response body does NOT contain exception.Message | No leak |
| 126 | Response body does NOT contain stack trace | No leak |
| 127 | Calls logger.LogError with the exception | Logging verification |

---

## P5 — Filter Logic

### `FluentValidationFilter`

```csharp
foreach (var argument in context.ActionArguments.Values)
{
    if (argument is null)              // B1: null → skip
        continue;
    var validator = GetService(...);
    if (validator is null)             // B2: no validator → skip
        continue;
    var result = await validator.ValidateAsync(...);
    if (!result.IsValid)               // B3: invalid → short-circuit 400
    {
        context.Result = ...;
        return;
    }
    // B4: valid → continue loop
}
await next();                          // B5: all passed → call next
```

| # | Test | Branch |
|---|------|--------|
| 128 | Null argument skipped, next() called | B1 → B5 |
| 129 | No validator registered, next() called | B2 → B5 |
| 130 | Validation passes, next() called | B4 → B5 |
| 131 | Validation fails, short-circuits with 400 ValidationProblemDetails | B3 |
| 132 | Empty arguments, next() called | B5 directly |
| 133 | Multiple args: first valid, second invalid → short-circuits on second | B4 then B3 |
| 134 | Multiple args: both valid → next() called | B4 + B4 → B5 |

### `DbTransactionFilter`

```csharp
if (Method == GET)           // B1: GET → skip transaction
{
    await next(); return;
}
try
{
    var executed = await next();
    bool isError =
        executed.Exception != null                               // B2: exception on context
        || executed.Result is IStatusCodeActionResult { >= 400 } // B3: 4xx result
    if (isError) rollback;   // B4: rollback (no rethrow)
    else commit;             // B5: commit
}
catch                        // B6: exception thrown → rollback + rethrow
{
    rollback; throw;
}
```

| # | Test | Branch |
|---|------|--------|
| 135 | GET request skips transaction, calls next() | B1 |
| 136 | POST with OkResult → commits | B5 |
| 137 | POST with OkObjectResult → commits | B5 |
| 138 | POST with NotFoundResult (StatusCode=404) → rolls back | B3 → B4 |
| 139 | POST with BadRequestResult (StatusCode=400) → rolls back | B3 → B4 |
| 140 | Action sets exception on context → rolls back (no rethrow) | B2 → B4 |
| 141 | Action throws exception → catch block rolls back + rethrows | B6 |
| 142 | Result without IStatusCodeActionResult (e.g. JsonResult) → commits | B5 |

---

## P6 — TokenService & CurrentUser

### `TokenService.GenerateJWT`

No branches — always generates a token.

| # | Test | Claim verified |
|---|------|----------------|
| 143 | Token contains NameIdentifier = userId | Identity claim |
| 144 | Token contains Role = role | Authorization claim |
| 145 | Token contains Email = email | Profile claim |
| 146 | Token contains GivenName = name | Profile claim |
| 147 | Token Issuer matches configured issuer | Validation parameter |
| 148 | Token Audience matches configured audience | Validation parameter |
| 149 | Token expiry is ~1 day from now (±5 min tolerance) | Lifetime |
| 150 | Token has unique Jti (generate two, compare) | Non-replayability |

### `CurrentUser`

```csharp
var user = accessor.HttpContext?.User    // B1: HttpContext null → throw
    ?? throw new UnauthorizedAccessException();
UserId = Guid.Parse(FindFirstValue(NameIdentifier)!);  // B2: claim missing → exception
Role = FindFirstValue(Role)!;                           // B3: role missing → null
// B4: all valid → success
```

| # | Test | Branch |
|---|------|--------|
| 151 | Valid claims → UserId and Role populated | B4 |
| 152 | HttpContext is null → throws UnauthorizedAccessException | B1 |
| 153 | HttpContext.User is null → throws UnauthorizedAccessException | B1 (via ?.) |
| 154 | NameIdentifier claim missing → throws | B2 |
| 155 | NameIdentifier claim has invalid GUID → throws FormatException | B2 (parse failure) |

---

## Test Helpers

Reusable factory methods to reduce boilerplate:

```
TestData.CreateProduct(...)     → Product with sensible defaults
TestData.CreateBuyer(...)       → Buyer via Buyer.Create with defaults
TestData.CreateCartItem(...)    → CartItem with product and cart
TestData.CreateOrder(...)       → Order via Order.Create with defaults
TestData.CreateTransaction(...) → Transaction with defaults
TestData.ValidBuyerRegisterDto()   → BuyerRegisterRequestDto with valid data
TestData.ValidSellerRegisterDto()  → SellerRegisterRequestDto with valid data
TestData.ValidLoginDto()           → LoginRequestDto with valid data
TestData.ValidAddProductDto()      → AddProductDto with valid data
TestData.ValidUpdateProductDto()   → UpdateProductDto with valid data
```

These live in `Helpers/TestData.cs` in the test project.

---

## Project Structure

```
ECommerce.Tests/
├── Helpers/
│   └── TestData.cs
├── Domain/
│   ├── OrderTests.cs
│   ├── BuyerTests.cs
│   ├── ProductTests.cs
│   └── TransactionTests.cs
├── Validators/
│   ├── BuyerRegisterRequestValidatorTests.cs
│   ├── SellerRegisterRequestValidatorTests.cs
│   ├── LoginRequestValidatorTests.cs
│   ├── AddProductValidatorTests.cs
│   ├── UpdateProductValidatorTests.cs
│   ├── SetStockValidatorTests.cs
│   └── UpdateOrderValidatorTests.cs
├── Mappings/
│   ├── ProductMappingTests.cs
│   ├── OrderMappingTests.cs
│   └── ProductUpdateMappingTests.cs
├── Filters/
│   ├── FluentValidationFilterTests.cs
│   └── DbTransactionFilterTests.cs
├── Services/
│   ├── TokenServiceTests.cs
│   ├── CurrentUserTests.cs
│   ├── DomainExceptionHandlerTests.cs
│   └── UnhandledExceptionHandlerTests.cs
```

---

## Conventions

- **Naming:** `MethodName_Scenario_ExpectedResult` (e.g. `TransitionTo_InTransitToCancelled_Throws`)
- **Pattern:** Arrange-Act-Assert with blank line separators
- **One assertion per test** where practical — multiple assertions acceptable when verifying a single logical outcome across fields
- **No test interdependence** — each test creates its own data, never depends on execution order
- **Theory + InlineData** for parameterized tests (e.g. all invalid transitions, all boundary lengths)
- **Branch tag** — each test documents which code branch it covers

---

## Total: 155 unit tests across 6 categories

| Category | Tests | Coverage target |
|----------|-------|-----------------|
| P1 — Domain Entity Logic | 1–24 | All branches in state machine, factories, computed properties |
| P2 — Validators | 25–89 | Every rule × (pass + each fail mode) + When guard branches |
| P3 — Mappers | 90–115 | Null guards, field renames, ignored targets, nullable per-field branching |
| P4 — Exception Handlers | 116–127 | Domain vs non-domain dispatch, status codes, no-leak verification |
| P5 — Filters | 128–142 | Every branch in foreach/if chains, GET skip, commit/rollback paths |
| P6 — TokenService & CurrentUser | 143–155 | All claims, validation params, null/missing claim branches |

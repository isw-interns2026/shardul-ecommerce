# C4 Architecture Documentation — eCommerce Platform

## Level 1: System Context Diagram

Shows the eCommerce system as a whole and its relationships with external actors and systems.

```mermaid
C4Context
    title System Context Diagram — eCommerce Platform

    Person(buyer, "Buyer", "Browses products, manages cart, places orders, makes payments")
    Person(seller, "Seller", "Lists products, manages stock, fulfills orders")

    System(ecommerce, "eCommerce Platform", "Allows buyers to purchase products from sellers with online payment processing")

    System_Ext(stripe, "Stripe", "Handles payment processing, hosted checkout, and webhook notifications")
    SystemDb_Ext(postgres, "PostgreSQL", "Stores all application data: users, products, orders, transactions")

    Rel(buyer, ecommerce, "Browses, adds to cart, places orders", "HTTPS")
    Rel(seller, ecommerce, "Lists products, manages stock, fulfills orders", "HTTPS")
    Rel(ecommerce, stripe, "Creates checkout sessions", "HTTPS/API")
    Rel(stripe, ecommerce, "Sends payment outcome webhooks", "HTTPS/POST")
    Rel(ecommerce, postgres, "Reads/writes application data", "TCP/5432")
    Rel(buyer, stripe, "Completes payment on hosted checkout page", "HTTPS")
```

---

## Level 2: Container Diagram

Shows the major deployable units and how they communicate.

```mermaid
C4Container
    title Container Diagram — eCommerce Platform

    Person(buyer, "Buyer")
    Person(seller, "Seller")

    System_Boundary(ecommerce, "eCommerce Platform") {
        Container(frontend, "React Frontend", "React Router v7, Vite, TypeScript", "SPA for buyer-facing product browsing, cart management, and order tracking")
        Container(backend, "ASP.NET Core API", ".NET 10, Entity Framework Core", "REST API handling auth, products, cart, orders, payments, and stock reservation")
        Container(tickerq, "TickerQ Background Jobs", ".NET, TickerQ v10", "Runs scheduled jobs to expire stale stock reservations every 5 minutes")
        ContainerDb(appdb, "Application Database", "PostgreSQL", "Stores buyers, sellers, products, carts, orders, transactions")
        ContainerDb(tickerdb, "TickerQ Database", "PostgreSQL (ticker schema)", "Stores job schedules and execution state")
    }

    System_Ext(stripe, "Stripe API", "Payment processing")

    Rel(buyer, frontend, "Uses", "HTTPS")
    Rel(seller, backend, "Uses", "HTTPS (no seller frontend yet)")
    Rel(frontend, backend, "API calls", "HTTPS/JSON")
    Rel(backend, appdb, "Reads/writes", "Npgsql")
    Rel(backend, stripe, "Creates checkout sessions", "Stripe .NET SDK")
    Rel(stripe, backend, "Webhooks: payment confirmed/expired", "HTTPS/POST")
    Rel(tickerq, appdb, "Queries stale reservations", "Npgsql")
    Rel(tickerq, tickerdb, "Manages job state", "Npgsql")
    Rel(buyer, stripe, "Pays on hosted page", "HTTPS redirect")
```

---

## Level 3: Component Diagram — Backend API

Shows the internal structure of the ASP.NET Core API container.

```mermaid
C4Component
    title Component Diagram — ASP.NET Core Backend

    Container_Boundary(api, "ASP.NET Core API") {

        Component(authCtrl, "AuthController", "[AllowAnonymous]", "Buyer/seller registration and JWT login")
        Component(buyerProductsCtrl, "BuyerProductsController", "[Authorize: Buyer]", "Browse listed products")
        Component(buyerCartCtrl, "BuyerCartController", "[Authorize: Buyer]", "Cart CRUD + PlaceOrders checkout flow")
        Component(buyerOrdersCtrl, "BuyerOrdersController", "[Authorize: Buyer]", "View order history")
        Component(sellerProductsCtrl, "SellerProductsController", "[Authorize: Seller]", "Product CRUD, stock management")
        Component(sellerOrdersCtrl, "SellerOrdersController", "[Authorize: Seller]", "View/update order status")
        Component(webhookCtrl, "StripeWebhookController", "[AllowAnonymous]", "Receives Stripe payment webhooks")

        Component(authRepo, "AuthRepository", "IAuthRepository", "Stages buyer/seller creation, validates credentials")
        Component(productRepo, "ProductsRepository", "IProductsRepository", "Stages product creation, queries by seller/listing")
        Component(cartRepo, "CartRepository", "ICartRepository", "Cart item CRUD, buyer lookup")
        Component(orderRepo, "OrdersRepository", "IOrdersRepository", "Stages order creation, queries with mandatory user filter")
        Component(txRepo, "TransactionRepository", "ITransactionRepository", "Stages transaction creation from cart totals")

        Component(stockSvc, "StockReservationService", "IStockReservationService", "Reserve/confirm/release stock with optimistic concurrency retry")
        Component(stripeSvc, "StripeService", "IStripeService", "Creates Stripe checkout sessions")
        Component(tokenSvc, "TokenService", "ITokenService", "Generates JWT access tokens")
        Component(currentUser, "CurrentUser", "ICurrentUser", "Extracts user ID and role from JWT claims")

        Component(dbContext, "ECommerceDbContext", "DbContext + IUnitOfWork", "EF Core context with exception translation override")
        Component(txFilter, "DbTransactionFilter", "IAsyncActionFilter", "Wraps mutating requests in a DB transaction")
        Component(exHandler, "DomainExceptionHandler", "IExceptionHandler", "Translates DomainExceptions to HTTP ProblemDetails")

        Component(cleanupJob, "ReservationCleanupJob", "TickerQ Function", "Expires stale reservations every 5 minutes")
    }

    ContainerDb_Ext(db, "PostgreSQL")
    System_Ext(stripe, "Stripe API")

    Rel(authCtrl, authRepo, "Stages entities")
    Rel(authCtrl, tokenSvc, "Generates JWT")
    Rel(authCtrl, dbContext, "SaveChangesAsync")

    Rel(buyerCartCtrl, cartRepo, "Cart operations")
    Rel(buyerCartCtrl, stockSvc, "Reserve stock")
    Rel(buyerCartCtrl, txRepo, "Stage transaction")
    Rel(buyerCartCtrl, orderRepo, "Stage orders")
    Rel(buyerCartCtrl, stripeSvc, "Create checkout")
    Rel(buyerCartCtrl, dbContext, "SaveChangesAsync")

    Rel(sellerProductsCtrl, productRepo, "Product operations")
    Rel(sellerProductsCtrl, dbContext, "SaveChangesAsync")

    Rel(sellerOrdersCtrl, orderRepo, "Query/update orders")
    Rel(sellerOrdersCtrl, dbContext, "SaveChangesAsync")

    Rel(webhookCtrl, stockSvc, "Confirm/release reservation")

    Rel(stockSvc, dbContext, "SaveChangesAsync (with retry)")
    Rel(stripeSvc, stripe, "Checkout Sessions API")
    Rel(cleanupJob, stockSvc, "Release expired reservations")

    Rel(dbContext, db, "Npgsql")

    Rel(txFilter, dbContext, "Begin/Commit/Rollback transaction")
    Rel(exHandler, authCtrl, "Catches DomainExceptions")
```

---

## Level 4: Key Flow — PlaceOrders Checkout Sequence

Shows the detailed interaction for the most critical business flow.

```mermaid
sequenceDiagram
    participant B as Buyer (Frontend)
    participant C as BuyerCartController
    participant F as DbTransactionFilter
    participant CR as CartRepository
    participant SS as StockReservationService
    participant TR as TransactionRepository
    participant OR as OrdersRepository
    participant UoW as IUnitOfWork (DbContext)
    participant St as StripeService
    participant Stripe as Stripe API
    participant DB as PostgreSQL

    B->>C: POST /buyer/cart (PlaceOrders)
    F->>DB: BEGIN TRANSACTION

    C->>CR: GetBuyerCartItemsAsync(buyerId)
    CR->>DB: SELECT cart items + products
    DB-->>CR: CartItems with Products
    CR-->>C: List<CartItem>

    C->>CR: GetBuyerByIdAsync(buyerId)
    CR->>DB: SELECT buyer
    DB-->>CR: Buyer
    CR-->>C: Buyer

    Note over C,SS: Step 1: Reserve stock (flushes internally for concurrency)
    C->>SS: ReserveStockForCartItems(cartItems)
    SS->>DB: FindAsync each Product
    SS->>SS: Validate AvailableStock >= requested
    SS->>SS: product.ReservedCount += count
    SS->>UoW: SaveChangesAsync()
    UoW->>DB: UPDATE Products SET ReservedCount (with concurrency WHERE)
    Note over SS,DB: If DbUpdateConcurrencyException → RefreshConflictingProducts → retry

    Note over C,OR: Step 2: Stage transaction + orders (no flush)
    C->>TR: CreateTransactionForCartItems(cartItems)
    TR->>TR: dbContext.Add(transaction)
    TR-->>C: Transaction (tracked, not persisted)

    C->>OR: CreateOrdersForTransaction(cartItems, buyer, transaction)
    OR->>OR: dbContext.Add(order) for each cart item

    Note over C,UoW: Step 3: Flush transaction + orders together
    C->>UoW: SaveChangesAsync()
    UoW->>DB: INSERT Transaction + Orders (single round trip)

    Note over C,Stripe: Step 4: Create Stripe checkout
    C->>St: CreateCheckoutSessionAsync(transaction, cartItems)
    St->>Stripe: POST /v1/checkout/sessions
    Stripe-->>St: Session { Id, Url }
    St->>St: transaction.StripeSessionId = session.Id
    St-->>C: checkoutUrl

    Note over C,UoW: Step 5: Flush StripeSessionId
    C->>UoW: SaveChangesAsync()
    UoW->>DB: UPDATE Transaction SET StripeSessionId

    Note over C,CR: Step 6: Clear cart
    C->>CR: ClearCartAsync(buyerId)
    CR->>DB: ExecuteDeleteAsync (bypasses change tracker)

    F->>DB: COMMIT TRANSACTION
    C-->>B: 200 OK { checkoutUrl }

    Note over B,Stripe: Buyer redirected to Stripe hosted checkout
    B->>Stripe: Complete payment on hosted page
```

---

## Level 4: Key Flow — Stripe Webhook (Payment Confirmed)

```mermaid
sequenceDiagram
    participant Stripe as Stripe API
    participant W as StripeWebhookController
    participant F as DbTransactionFilter
    participant DB as PostgreSQL
    participant SS as StockReservationService

    Stripe->>W: POST /api/stripe/webhook (CheckoutSessionCompleted)
    F->>DB: BEGIN TRANSACTION

    W->>W: VerifyAndParseEvent (signature check)
    W->>DB: SELECT Transaction WHERE StripeSessionId = session.Id
    DB-->>W: Transaction

    W->>SS: ConfirmReservation(transactionId)
    SS->>DB: SELECT Transaction WHERE Id = transactionId
    SS->>SS: Guard: if Status != Processing → return
    SS->>DB: SELECT Orders + Products WHERE TransactionId
    SS->>SS: For each order: CountInStock -= count, ReservedCount -= count
    SS->>SS: order.MarkInTransit()
    SS->>SS: transaction.Status = Success
    SS->>DB: SaveChangesAsync (UPDATE Products, Orders, Transaction)
    Note over SS,DB: If DbUpdateConcurrencyException → RefreshConflictingProducts → retry

    F->>DB: COMMIT TRANSACTION
    W-->>Stripe: 200 OK (always)
```

---

## Level 4: Key Flow — Reservation Expiry (Background Job)

```mermaid
sequenceDiagram
    participant TQ as TickerQ Scheduler
    participant J as ReservationCleanupJob
    participant DB as PostgreSQL
    participant SS as StockReservationService

    TQ->>J: ExecuteAsync (every 5 minutes)
    J->>DB: SELECT * FROM Transactions WHERE Status = 'Processing'
    DB-->>J: All processing transactions

    J->>J: Filter in-memory: CreatedAt < (now - 15 min)
    Note over J: CreatedAt extracted from UUIDv7 primary key

    loop For each stale transaction
        J->>SS: ReleaseReservation(transactionId)
        SS->>DB: SELECT Transaction WHERE Id = transactionId
        SS->>SS: Guard: if Status != Processing → return
        SS->>DB: SELECT Orders + Products WHERE TransactionId
        SS->>SS: For each order: ReservedCount -= count
        SS->>SS: order.MarkCancelled()
        SS->>SS: transaction.Status = Expired
        SS->>DB: SaveChangesAsync
    end
```

---

## Data Model (Entity Relationships)

```mermaid
erDiagram
    Buyer ||--|| Cart : "has one"
    Cart ||--o{ CartItem : "contains"
    CartItem }o--|| Product : "references"
    Product }o--|| Seller : "owned by"
    Order }o--|| Buyer : "placed by"
    Order }o--|| Seller : "fulfilled by"
    Order }o--|| Product : "for product"
    Order }o--|| Transaction : "paid via"

    Buyer {
        uuid Id PK "UUIDv7"
        string Name
        string Email UK
        string PasswordHash
        string Address
    }

    Seller {
        uuid Id PK "UUIDv7"
        string Name
        string Email UK
        string PasswordHash
        string BankAccountNumber
    }

    Product {
        uuid Id PK "UUIDv7"
        uuid SellerId FK
        string Sku
        string Name
        decimal Price "CHECK > 0"
        int CountInStock "CHECK >= 0, concurrency token"
        int ReservedCount "CHECK >= 0 AND <= CountInStock, concurrency token"
        string Description "nullable"
        string ImageUrl "nullable"
        bool IsListed
    }

    Cart {
        uuid Id PK "UUIDv7"
        uuid BuyerId FK_UK "one-to-one"
    }

    CartItem {
        uuid Id PK "UUIDv7"
        uuid CartId FK
        uuid ProductId FK
        int Count
    }

    Order {
        uuid Id PK "UUIDv7"
        uuid ProductId FK
        uuid BuyerId FK
        uuid SellerId FK
        uuid TransactionId FK
        int Count "CHECK > 0"
        decimal Total "CHECK >= 0"
        string Address
        string Status "enum: AwaitingPayment, InTransit, Delivered, Cancelled"
    }

    Transaction {
        uuid Id PK "UUIDv7"
        decimal Amount
        string Status "enum: Processing, Success, Failed, Expired"
        string StripeSessionId UK "nullable"
    }
```

---

## Exception Pipeline

```mermaid
flowchart LR
    PG[PostgreSQL Constraint Violation] --> SCO[SaveChangesAsync Override]
    SCO -->|Unique violation mapped| DE[DomainException]
    SCO -->|Unmapped constraint| Raw[DbUpdateException → 500]

    BL[Business Logic] -->|Validation failures| DE

    DE --> DEH[DomainExceptionHandler]
    DEH --> HTTP[HTTP Response with ProblemDetails]

    CC[Concurrency Conflict] --> Retry[ExecuteWithRetry]
    Retry -->|Refresh products| Retry
    Retry -->|Max retries exceeded| Raw2[DbUpdateConcurrencyException → 500]

    subgraph "Infrastructure Boundary (DbContext)"
        SCO
    end

    subgraph "Middleware Pipeline"
        DEH
    end
```

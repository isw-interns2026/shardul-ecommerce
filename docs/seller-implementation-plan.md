# Seller Side Implementation Plan

## API Endpoints (from controllers)

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `POST /auth/seller/register` | POST | Register (name, email, password, bankAccountNumber) |
| `POST /auth/seller/login` | POST | Login (email, password) → JWT with role=Seller |
| `GET /seller/products` | GET | List all seller's products |
| `GET /seller/products/:productId` | GET | Get single product detail |
| `POST /seller/products` | POST | Add new product |
| `PATCH /seller/products/:productId` | PATCH | Update product fields (partial) |
| `PUT /seller/products/:productId/stock` | PUT | Set stock count |
| `GET /seller/orders` | GET | List all orders for seller's products |
| `GET /seller/orders/:orderId` | GET | Get single order detail |
| `PATCH /seller/orders/:orderId` | PATCH | Update order status (InTransit → Delivered) |

---

## Phase 1 — Auth & Routing Foundation

| # | Task | Status | Files |
|---|------|--------|-------|
| 1.1 | **Add seller TypeScript types** to `ResponseDto.ts` — `SellerProductResponseDto` (id, sku, name, price, countInStock, description, imageUrl, isListed) and `SellerOrderResponseDto` (orderId, orderValue, productCount, productId, productName, productSku, deliveryAddress) | 🔲 Open | `types/ResponseDto.ts` |
| 1.2 | **Add seller auth service functions** — `sendSellerLoginRequest`, `sendSellerRegisterRequest` | 🔲 Open | `services/AuthService.ts` |
| 1.3 | **Implement seller login page** — mirror `buyer_login.tsx`, POST to `/auth/seller/login`, redirect to `/seller` | 🔲 Open | `routes/auth/seller_login.tsx` |
| 1.4 | **Implement seller register page** — fields: Name, Email, Password, Bank Account Number. Redirect to seller login on success | 🔲 Open | `routes/auth/seller_register.tsx` |
| 1.5 | **Create seller navbar layout** — links to "My Products", "Orders", Logout. Separate from buyer navbar (different auth guard — check JWT role is Seller) | 🔲 Open | `layouts/seller_navbar.tsx` |
| 1.6 | **Register all seller routes** in `routes.ts` — uncomment auth routes, add seller layout with nested routes | 🔲 Open | `routes.ts` |

## Phase 2 — Product Management (CRUD)

| # | Task | Status | Files |
|---|------|--------|-------|
| 2.1 | **Seller product list page** — GET `/seller/products`, display as a table (name, SKU, price, stock, listed status). Include "Add Product" button | 🔲 Open | `routes/seller/seller_products.tsx` |
| 2.2 | **Add product page/dialog** — form with fields: SKU, Name, Price, Stock, Description (optional), Image URL (optional), Is Listed toggle. POST to `/seller/products` | 🔲 Open | `routes/seller/add_product.tsx` |
| 2.3 | **Edit product page** — GET `/seller/products/:productId` to prefill form. PATCH to `/seller/products/:productId` for field updates. Include stock management via PUT `/seller/products/:productId/stock` | 🔲 Open | `routes/seller/edit_product.tsx` |
| 2.4 | **Toggle listing status** — inline action on the product table to quickly list/unlist a product (PATCH with `{ isListed: true/false }`) | 🔲 Open | Part of `seller_products.tsx` |

## Phase 3 — Order Management

| # | Task | Status | Files |
|---|------|--------|-------|
| 3.1 | **Seller orders list page** — GET `/seller/orders`, display as table/cards (order ID, product name, SKU, qty, total, delivery address). Sort by most recent (UUID v7) | 🔲 Open | `routes/seller/seller_orders.tsx` |
| 3.2 | **Mark as Delivered action** — PATCH `/seller/orders/:orderId` with `{ status: "Delivered" }`. Only available for orders currently "InTransit". Use a confirmation dialog | 🔲 Open | Part of `seller_orders.tsx` |

## Phase 4 — Polish

| # | Task | Status | Files |
|---|------|--------|-------|
| 4.1 | **Update home page** — add "Seller Login / Register" buttons alongside existing buyer buttons | 🔲 Open | `routes/home.tsx` |
| 4.2 | **Navigation progress bar** — reuse the same pattern from buyer navbar | 🔲 Open | `layouts/seller_navbar.tsx` |
| 4.3 | **Error handling** — toast notifications for all API failures, same pattern as buyer side | 🔲 Open | All seller route files |
| 4.4 | **Empty states** — "No products yet" and "No orders yet" placeholder UI | 🔲 Open | `seller_products.tsx`, `seller_orders.tsx` |

---

## Route Structure (for `routes.ts`)

```ts
// Auth
route("auth/seller/login",    "routes/auth/seller_login.tsx"),
route("auth/seller/register", "routes/auth/seller_register.tsx"),

// Seller dashboard (behind seller navbar layout)
layout("layouts/seller_navbar.tsx", [
  ...prefix("seller", [
    index("routes/seller/seller_products.tsx"),          // /seller
    route("products/new",         "routes/seller/add_product.tsx"),   // /seller/products/new
    route("products/:productId",  "routes/seller/edit_product.tsx"),  // /seller/products/:id
    route("orders",               "routes/seller/seller_orders.tsx"), // /seller/orders
  ]),
]),
```

---

## Key Design Decisions

- **Separate navbar layout** — seller and buyer have different nav links, different auth guards (check JWT role), and no overlap
- **Table view for products** — sellers need data density (SKU, stock, listed status) rather than the card grid buyers see
- **Inline stock editing** — the dedicated `PUT /stock` endpoint suggests stock updates should be quick/inline, not buried in a full edit form
- **Order status is read-only except InTransit → Delivered** — the only valid seller transition. AwaitingPayment → InTransit and → Cancelled are system-driven (Stripe webhook)
- **No seller-side order status column in DTO** — the backend `Order` entity has a `Status` field but `SellerOrderResponseDto` doesn't expose it. Consider adding `OrderStatus` to the DTO so the frontend can show status and conditionally render the "Mark Delivered" button

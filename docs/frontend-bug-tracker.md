# Frontend Bug Tracker

## Bugs

| # | Description | Severity | Status | File(s) |
|---|-------------|----------|--------|---------|
| 1 | Place Order spam click creates duplicate orders | P1 | ✅ Fixed | `view_cart.tsx`, `place_order.tsx` |
| 2 | Cart quantity increment has no upper bound (exceeds available stock) | P1 | ✅ Fixed | `view_cart.tsx` |
| 3 | No error handling in any `clientLoader` — API failures crash to error boundary | P2 | ✅ Fixed | `buyer_home.tsx`, `view_product.tsx`, `view_cart.tsx`, `view_orders.tsx` |
| 4 | No error handling in `clientAction` for cart/order operations | P2 | ✅ Fixed | `view_cart.tsx`, `delete_from_cart.tsx`, `place_order.tsx`, `view_product.tsx` |
| 5 | `add_to_cart.tsx` is a dead route — nothing navigates to it | P3 | ✅ Fixed | Deleted `add_to_cart.tsx`, removed from `routes.ts` |
| 6 | Login/register errors use `alert()` — poor UX, swallows all error types | P2 | ✅ Fixed | `buyer_login.tsx`, `buyer_register.tsx` |
| 7 | Logout race condition — `removeItem` and navigation fire simultaneously | P5 | ✅ Fixed | `navbar.tsx` |
| 8 | Auth guard effect runs on every render, not just mount | P5 | ✅ Fixed | `navbar.tsx` |

## Features

| # | Description | Priority | Status | File(s) |
|---|-------------|----------|--------|---------|
| F1 | Loading states / skeleton loaders for all pages | P3 | 🔲 Open | All route files |
| F2 | Toast notifications via sonner | P3 | ✅ Done | `root.tsx` (Toaster wired), used in all error paths |
| F3 | Inline error display with retry for API failures | P3 | ✅ Done | `view_product.tsx` (notFound state), others use toast + empty state |
| F4 | Place order confirmation dialog | P4 | 🔲 Open | `view_cart.tsx` |
| F5 | Order success page after Stripe redirect | P4 | 🔲 Open | New route |
| F6 | Cart badge count in navbar | P4 | 🔲 Open | `navbar.tsx` |

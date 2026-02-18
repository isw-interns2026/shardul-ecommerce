# Frontend Bug Tracker

## Bugs

| # | Description | Severity | Status | File(s) |
|---|-------------|----------|--------|---------|
| 1 | Place Order spam click creates duplicate orders | P1 | ✅ Fixed | `view_cart.tsx`, `place_order.tsx` |
| 2 | Cart quantity increment has no upper bound (exceeds available stock) | P1 | ✅ Fixed | `view_cart.tsx` |
| 3 | No error handling in any `clientLoader` — API failures crash to error boundary | P2 | 🔲 Open | All route files |
| 4 | No error handling in `clientAction` for cart operations | P2 | 🔲 Open | `view_cart.tsx`, `delete_from_cart.tsx` |
| 5 | `add_to_cart.tsx` is a dead route — nothing navigates to it | P3 | 🔲 Open | `add_to_cart.tsx`, `routes.ts` |
| 6 | Login/register errors use `alert()` — poor UX, swallows all error types | P2 | 🔲 Open | `buyer_login.tsx`, `buyer_register.tsx` |
| 7 | Logout race condition — `removeItem` and navigation fire simultaneously | P5 | 🔲 Open | `navbar.tsx` |
| 8 | Auth guard effect runs on every render, not just mount | P5 | 🔲 Open | `navbar.tsx` |

## Features

| # | Description | Priority | Status | File(s) |
|---|-------------|----------|--------|---------|
| F1 | Loading states / skeleton loaders for all pages | P3 | 🔲 Open | All route files |
| F2 | Toast notifications via sonner (already in package.json) | P3 | 🔲 Open | Global |
| F3 | Inline error display with retry for API failures | P3 | 🔲 Open | All route files |
| F4 | Place order confirmation dialog | P4 | 🔲 Open | `view_cart.tsx` |
| F5 | Order success page after Stripe redirect | P4 | 🔲 Open | New route |
| F6 | Cart badge count in navbar | P4 | 🔲 Open | `navbar.tsx` |

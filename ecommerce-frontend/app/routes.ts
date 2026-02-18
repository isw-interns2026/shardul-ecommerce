import {
  index,
  layout,
  prefix,
  route,
} from "@react-router/dev/routes";

export default [
  index("routes/home.tsx"),
  route("/auth/buyer/login", "routes/auth/buyer_login.tsx"),
  route("auth/buyer/register", "routes/auth/buyer_register.tsx"),
  // route("auth/seller/login", "routes/auth/seller_login.tsx"),
  // route("auth/seller/register", "routes/auth/seller_register.tsx")
layout("layouts/navbar.tsx", [
    ...prefix("buyer", [
      index("routes/buyer/buyer_home.tsx"),

      route("products/:productId", "routes/buyer/view_product.tsx"),

      route("cart", "routes/buyer/view_cart.tsx"),
      route("orders", "routes/buyer/view_orders.tsx"),
      route("cart/place_order", "routes/buyer/place_order.tsx"),
      route("cart/delete/:productId", "routes/buyer/delete_from_cart.tsx")
    ]),
  ]),
];

import { type RouteConfig, index, layout, prefix, route } from "@react-router/dev/routes";

export default [
    index("routes/home.tsx"),
    route("/auth/buyer/login", "routes/auth/buyer_login.tsx"),
    // route("auth/seller/login", "routes/auth/seller_login.tsx"),
    // route("auth/buyer/register", "routes/auth/buyer_register.tsx"),
    // route("auth/seller/register", "routes/auth/seller_register.tsx")
    ...prefix("/buyer", [
        index("routes/buyer/buyer_home.tsx"),
        route("/products/:productId", "routes/buyer/view_product.tsx")
    ]),
] satisfies RouteConfig;
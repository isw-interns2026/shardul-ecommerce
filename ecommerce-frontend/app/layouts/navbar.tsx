import { NavLink, Outlet, useNavigate, useNavigation } from "react-router";
import { cn } from "~/lib/utils";
import { useEffect, useCallback } from "react";
import { CartProvider, useCart } from "~/context/CartContext";

export default function NavbarLayout() {
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (!token) {
      void navigate("/auth/buyer/login", { replace: true });
    }
  }, []); // Run only on mount, not on every render

  // Don't render anything while redirecting
  if (!localStorage.getItem("accessToken")) {
    return null;
  }

  return (
    <CartProvider>
      <div className="min-h-screen flex flex-col">
        <NavBar />
        <main className="flex-1">
          <NavigationProgress />
          <Outlet />
        </main>
      </div>
    </CartProvider>
  );
}

function NavigationProgress() {
  const navigation = useNavigation();
  const isNavigating = navigation.state === "loading";

  return (
    <div
      role="progressbar"
      aria-hidden={!isNavigating}
      className={cn(
        "fixed top-14 left-0 z-40 h-0.5 bg-primary transition-all duration-300 ease-out",
        isNavigating ? "w-2/3 opacity-100" : "w-full opacity-0",
      )}
    />
  );
}

export function NavBar() {
  const navigate = useNavigate();
  const { cartCount } = useCart();

  const handleLogout = useCallback(
    (e: React.MouseEvent) => {
      e.preventDefault();
      localStorage.removeItem("accessToken");
      void navigate("/", { replace: true });
    },
    [navigate],
  );

  return (
    <>
      {/* Navbar */}
      <nav className="fixed top-0 left-0 z-50 w-full h-14 bg-background border-b shadow-sm">
        <div className="mx-auto flex h-full max-w-7xl items-center gap-6 px-6">
          <NavItem to="/buyer" end>
            All Products
          </NavItem>
          <NavItem to="/buyer/cart">
            Cart
            {cartCount > 0 && (
              <span className="ml-1.5 inline-flex items-center justify-center rounded-full bg-primary text-primary-foreground text-[10px] font-bold min-w-4.5 h-4.5 px-1">
                {cartCount > 99 ? "99+" : cartCount}
              </span>
            )}
          </NavItem>
          <NavItem to="/buyer/orders">Orders</NavItem>

          <div className="ml-auto">
            <a
              href="/"
              onClick={handleLogout}
              className="text-sm font-medium text-destructive hover:underline cursor-pointer"
            >
              Logout
            </a>
          </div>
        </div>
      </nav>

      {/* Spacer so content doesn't go under navbar */}
      <div className="h-14" />
    </>
  );
}

function NavItem({
  to,
  children,
  end = false,
}: {
  to: string;
  children: React.ReactNode;
  end?: boolean;
}) {
  return (
    <NavLink
      to={to}
      end={end}
      className={({ isActive }) =>
        cn(
          "text-sm font-medium transition-colors hover:text-primary",
          isActive
            ? "text-primary underline underline-offset-4"
            : "text-muted-foreground",
        )
      }
    >
      {children}
    </NavLink>
  );
}

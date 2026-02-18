import { NavLink, Outlet, useNavigate, useNavigation } from "react-router";
import { cn } from "~/lib/utils";
import { useEffect, useCallback } from "react";
import { CartProvider, useCart } from "~/context/CartContext";
import {
  ShoppingBag,
  LogOut,
  Package,
  ShoppingCart,
  ClipboardList,
} from "lucide-react";
import { Button } from "~/components/ui/button";

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
      <nav className="fixed top-0 left-0 z-50 w-full h-14 bg-background/80 backdrop-blur-lg border-b">
        <div className="mx-auto flex h-full max-w-7xl items-center gap-1 px-6">
          {/* Brand */}
          <NavLink
            to="/buyer"
            className="flex items-center gap-2 mr-6 shrink-0"
          >
            <div className="p-1.5 rounded-lg bg-primary">
              <ShoppingBag className="h-4 w-4 text-primary-foreground" />
            </div>
            <span className="text-lg font-extrabold tracking-tight text-foreground">
              E-Shop
            </span>
          </NavLink>

          <NavItem to="/buyer" end icon={<Package className="h-4 w-4" />}>
            Products
          </NavItem>
          <NavItem to="/buyer/cart" icon={<ShoppingCart className="h-4 w-4" />}>
            Cart
            {cartCount > 0 && (
              <span className="ml-1 inline-flex items-center justify-center rounded-full bg-primary text-primary-foreground text-[10px] font-bold min-w-4.5 h-4.5 px-1 leading-none">
                {cartCount > 99 ? "99+" : cartCount}
              </span>
            )}
          </NavItem>
          <NavItem
            to="/buyer/orders"
            icon={<ClipboardList className="h-4 w-4" />}
          >
            Orders
          </NavItem>

          <div className="ml-auto">
            <Button
              variant="ghost"
              size="sm"
              onClick={handleLogout}
              className="text-muted-foreground hover:text-destructive hover:bg-destructive/10 gap-2"
            >
              <LogOut className="h-4 w-4" />
              <span className="hidden sm:inline">Logout</span>
            </Button>
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
  icon,
}: {
  to: string;
  children: React.ReactNode;
  end?: boolean;
  icon?: React.ReactNode;
}) {
  return (
    <NavLink
      to={to}
      end={end}
      className={({ isActive }) =>
        cn(
          "flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-sm font-medium transition-all",
          isActive
            ? "bg-primary/10 text-primary"
            : "text-muted-foreground hover:text-foreground hover:bg-muted",
        )
      }
    >
      {icon}
      {children}
    </NavLink>
  );
}

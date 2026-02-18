import { NavLink, Outlet, useNavigate, useNavigation } from "react-router";
import { cn } from "~/lib/utils";
import { useEffect, useCallback } from "react";

export default function SellerNavbarLayout() {
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (!token) {
      void navigate("/auth/seller/login", { replace: true });
    }
  }, []);

  if (!localStorage.getItem("accessToken")) {
    return null;
  }

  return (
    <div className="min-h-screen flex flex-col">
      <SellerNavBar />
      <main className="flex-1">
        <NavigationProgress />
        <Outlet />
      </main>
    </div>
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

function SellerNavBar() {
  const navigate = useNavigate();

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
      <nav className="fixed top-0 left-0 z-50 w-full h-14 bg-background border-b shadow-sm">
        <div className="mx-auto flex h-full max-w-7xl items-center gap-6 px-6">
          <span className="text-xs font-bold uppercase tracking-widest text-primary/60 mr-2">
            Seller
          </span>
          <NavItem to="/seller" end>
            My Products
          </NavItem>
          <NavItem to="/seller/orders">Orders</NavItem>

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

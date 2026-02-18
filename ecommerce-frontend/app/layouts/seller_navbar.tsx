import { NavLink, Outlet, useNavigate, useNavigation } from "react-router";
import { cn } from "~/lib/utils";
import { useEffect, useCallback } from "react";
import { Store, LogOut, Package, ClipboardList } from "lucide-react";
import { Button } from "~/components/ui/button";
import { Badge } from "~/components/ui/badge";

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
      <nav className="fixed top-0 left-0 z-50 w-full h-14 bg-background/80 backdrop-blur-lg border-b">
        <div className="mx-auto flex h-full max-w-7xl items-center gap-1 px-6">
          {/* Brand */}
          <NavLink
            to="/seller"
            className="flex items-center gap-2 mr-4 shrink-0"
          >
            <div className="p-1.5 rounded-lg bg-primary">
              <Store className="h-4 w-4 text-primary-foreground" />
            </div>
            <span className="text-lg font-extrabold tracking-tight text-foreground">
              E-Shop
            </span>
            <Badge
              variant="outline"
              className="text-[10px] px-1.5 py-0 h-5 font-bold uppercase tracking-wider border-primary/30 text-primary"
            >
              Seller
            </Badge>
          </NavLink>

          <NavItem to="/seller" end icon={<Package className="h-4 w-4" />}>
            My Products
          </NavItem>
          <NavItem
            to="/seller/orders"
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

import { NavLink, Outlet, useNavigate } from "react-router";
import { cn } from "~/lib/utils";
import { useEffect } from "react";

export default function NavbarLayout() {
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem("accessToken");
    if (!token) {
      void navigate("/auth/buyer/login", { replace: true });
    }
  }, [navigate]);

  // Don't render anything while redirecting
  if (!localStorage.getItem("accessToken")) {
    return null;
  }

  return (
    <div className="min-h-screen flex flex-col">
      <NavBar />
      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  );
}

export function NavBar() {
  return (
    <>
      {/* Navbar */}
      <nav className="fixed top-0 left-0 z-50 w-full h-14 bg-background border-b shadow-sm">
        <div className="mx-auto flex h-full max-w-7xl items-center gap-6 px-6">
          <NavItem to="/buyer" end>All Products</NavItem>
          <NavItem to="/buyer/cart">Cart</NavItem>
          <NavItem to="/buyer/orders">Orders</NavItem>

          <div className="ml-auto">
            <NavLink
              to="/"
              onClick={() => localStorage.removeItem("accessToken")}
              className="text-sm font-medium text-destructive hover:underline"
            >
              Logout
            </NavLink>
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
            : "text-muted-foreground"
        )
      }
    >
      {children}
    </NavLink>
  );
}

/* eslint-disable @typescript-eslint/no-misused-promises */
import type { Route } from "./+types/home";
import { Button } from "~/components/ui/button";
import { useNavigate } from "react-router";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "~/components/ui/card";
import {
  ShoppingBag,
  LogIn,
  UserPlus,
  ArrowRight,
  Store,
  Zap,
  Shield,
  Truck,
} from "lucide-react";

export function meta({}: Route.MetaArgs) {
  return [
    { title: "E-Shop — The Future of Retail" },
    { name: "description", content: "Your one-stop shop for everything." },
  ];
}

export default function Home() {
  const navigate = useNavigate();

  return (
    <div className="relative min-h-screen w-full flex flex-col overflow-hidden bg-background">
      {/* Decorative Background */}
      <div className="absolute inset-0 overflow-hidden -z-10">
        <div className="absolute -top-[20%] -left-[10%] w-[50%] h-[50%] rounded-full bg-primary/5 blur-3xl" />
        <div className="absolute top-[30%] -right-[15%] w-[45%] h-[45%] rounded-full bg-primary/8 blur-3xl" />
        <div className="absolute -bottom-[10%] left-[20%] w-[35%] h-[35%] rounded-full bg-primary/4 blur-3xl" />
      </div>

      {/* Hero Section */}
      <div className="flex-1 flex items-center">
        <div className="container max-w-6xl mx-auto px-4 py-16 lg:py-24">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 items-center">
            {/* Left Side: Branding */}
            <div className="space-y-8 text-center lg:text-left animate-slide-up">
              <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-primary/10 text-primary text-sm font-semibold">
                <Zap className="h-4 w-4" />
                India&apos;s fastest growing marketplace
              </div>

              <h1 className="text-5xl lg:text-7xl font-black tracking-tighter text-foreground leading-[0.9]">
                The{" "}
                <span className="text-primary relative">
                  Future
                  <span className="absolute -bottom-1 left-0 w-full h-2 bg-primary/20 rounded-full" />
                </span>{" "}
                of Retail.
              </h1>

              <p className="text-lg text-muted-foreground max-w-md mx-auto lg:mx-0 leading-relaxed">
                Buy and sell products with confidence. Lightning-fast checkout,
                secure payments, and reliable delivery — all in one place.
              </p>

              {/* Trust Badges */}
              <div className="flex flex-wrap gap-6 justify-center lg:justify-start text-sm text-muted-foreground">
                <div className="flex items-center gap-2">
                  <div className="p-1.5 rounded-md bg-primary/10">
                    <Shield className="h-4 w-4 text-primary" />
                  </div>
                  <span className="font-medium">Secure Payments</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className="p-1.5 rounded-md bg-primary/10">
                    <Truck className="h-4 w-4 text-primary" />
                  </div>
                  <span className="font-medium">Fast Delivery</span>
                </div>
                <div className="flex items-center gap-2">
                  <div className="p-1.5 rounded-md bg-primary/10">
                    <ShoppingBag className="h-4 w-4 text-primary" />
                  </div>
                  <span className="font-medium">Wide Selection</span>
                </div>
              </div>
            </div>

            {/* Right Side: Action Cards */}
            <div
              className="flex flex-col gap-5 justify-center lg:justify-end items-center lg:items-end animate-slide-up"
              style={{ animationDelay: "100ms" }}
            >
              {/* Buyer Card */}
              <Card className="w-full max-w-md border-2 shadow-2xl shadow-primary/10 hover:shadow-primary/15 transition-shadow duration-300">
                <CardHeader className="space-y-1 pb-4">
                  <div className="flex items-center gap-3">
                    <div className="p-2.5 rounded-xl bg-primary/10">
                      <ShoppingBag className="h-5 w-5 text-primary" />
                    </div>
                    <div>
                      <CardTitle className="text-xl font-bold">
                        Start Shopping
                      </CardTitle>
                      <CardDescription>
                        Browse products and place orders
                      </CardDescription>
                    </div>
                  </div>
                </CardHeader>
                <CardContent className="flex flex-col gap-3 pt-0">
                  <Button
                    size="lg"
                    className="w-full h-12 text-base font-bold transition-all hover:translate-x-0.5 group"
                    onClick={() => navigate("/auth/buyer/login")}
                  >
                    <LogIn className="mr-2 h-5 w-5" />
                    Buyer Login
                    <ArrowRight className="ml-auto h-4 w-4 opacity-50 group-hover:opacity-100 transition-opacity" />
                  </Button>

                  <Button
                    variant="outline"
                    size="lg"
                    className="w-full h-11 text-base font-semibold border-2 hover:bg-primary/5 transition-all"
                    onClick={() => navigate("/auth/buyer/register")}
                  >
                    <UserPlus className="mr-2 h-5 w-5" />
                    Create Buyer Account
                  </Button>
                </CardContent>
              </Card>

              {/* Seller Card */}
              <Card className="w-full max-w-md border hover:border-primary/30 shadow-md hover:shadow-lg transition-all duration-300">
                <CardHeader className="space-y-1 pb-4">
                  <div className="flex items-center gap-3">
                    <div className="p-2.5 rounded-xl bg-secondary">
                      <Store className="h-5 w-5 text-secondary-foreground" />
                    </div>
                    <div>
                      <CardTitle className="text-lg font-bold">
                        Sell on E-Shop
                      </CardTitle>
                      <CardDescription>
                        List products and manage orders
                      </CardDescription>
                    </div>
                  </div>
                </CardHeader>
                <CardContent className="flex flex-col gap-3 pt-0">
                  <Button
                    size="lg"
                    variant="secondary"
                    className="w-full h-11 text-base font-bold transition-all hover:translate-x-0.5 group"
                    onClick={() => navigate("/auth/seller/login")}
                  >
                    <Store className="mr-2 h-5 w-5" />
                    Seller Login
                    <ArrowRight className="ml-auto h-4 w-4 opacity-50 group-hover:opacity-100 transition-opacity" />
                  </Button>

                  <Button
                    variant="ghost"
                    size="lg"
                    className="w-full h-10 text-sm font-semibold hover:bg-secondary/80 transition-all"
                    onClick={() => navigate("/auth/seller/register")}
                  >
                    <UserPlus className="mr-2 h-4 w-4" />
                    Register as Seller
                  </Button>
                </CardContent>
              </Card>

              <p className="text-center text-xs text-muted-foreground max-w-md">
                By continuing, you agree to our Terms of Service and Privacy
                Policy.
              </p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

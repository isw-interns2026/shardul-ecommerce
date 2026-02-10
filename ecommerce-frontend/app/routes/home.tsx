/* eslint-disable @typescript-eslint/no-misused-promises */
import type { Route } from "./+types/home";
import { Button } from "~/components/ui/button";
import { useNavigate } from "react-router";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "~/components/ui/card";
import { ShoppingBag, LogIn, UserPlus, ArrowRight } from "lucide-react";

export function meta({}: Route.MetaArgs) {
  return [
    { title: "Welcome to E-Shop" },
    { name: "description", content: "Your one-stop shop for everything." },
  ];
}

export default function Home() {
  const navigate = useNavigate();

  return (
    <div className="relative min-h-screen w-full flex items-center justify-center overflow-hidden bg-slate-50">
      {/* Decorative Background Elements */}
      <div className="absolute top-0 left-0 w-full h-full overflow-hidden -z-10">
        <div className="absolute -top-[10%] -left-[10%] w-[40%] h-[40%] rounded-full bg-primary/5 blur-3xl" />
        <div className="absolute -bottom-[10%] -right-[10%] w-[40%] h-[40%] rounded-full bg-primary/10 blur-3xl" />
      </div>

      <div className="container max-w-6xl px-4 py-12">
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          
          {/* Left Side: Branding/Hero Text */}
          <div className="space-y-6 text-center lg:text-left">
            <h1 className="text-5xl lg:text-7xl font-black tracking-tighter text-slate-900">
             The <span className="text-primary">Future</span> of Retail.
            </h1>
          </div>

          {/* Right Side: Action Card */}
          <div className="flex justify-center lg:justify-end">
            <Card className="w-full max-w-md border-2 shadow-2xl shadow-primary/10">
              <CardHeader className="space-y-1 text-center">
                <CardTitle className="text-2xl font-bold">Get Started</CardTitle>
                <CardDescription>
                  Login or create an account to start shopping
                </CardDescription>
              </CardHeader>
              <CardContent className="flex flex-col gap-4 p-6">
                <Button 
                  size="lg" 
                  className="w-full h-12 text-base font-bold transition-all hover:translate-x-1"
                  onClick={() => navigate("/auth/buyer/login")}
                >
                  <LogIn className="mr-2 h-5 w-5" />
                  Login
                  <ArrowRight className="ml-auto h-4 w-4 opacity-50" />
                </Button>

                <div className="relative my-2">
                  <div className="absolute inset-0 flex items-center">
                    <span className="w-full border-t" />
                  </div>
                  <div className="relative flex justify-center text-xs uppercase">
                    <span className="bg-background px-2 text-muted-foreground">New here?</span>
                  </div>
                </div>

                <Button 
                  variant="outline" 
                  size="lg" 
                  className="w-full h-12 text-base font-bold border-2 hover:bg-primary/5 transition-all"
                  onClick={() => navigate("/auth/buyer/register")}
                >
                  <UserPlus className="mr-2 h-5 w-5" />
                  Create Account
                </Button>
                
                <p className="text-center text-xs text-muted-foreground mt-4">
                  By continuing, you agree to our Terms of Service and Privacy Policy.
                </p>
              </CardContent>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
}

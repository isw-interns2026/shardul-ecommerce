/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import { Form, redirect, useNavigation, Link } from "react-router";
import { Button } from "~/components/ui/button";
import { Field, FieldLabel } from "~/components/ui/field";
import { Input } from "~/components/ui/input";
import type { Route } from "./+types/buyer_login";
import { sendBuyerLoginRequest } from "~/services/AuthService";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "~/components/ui/card";
import { LogIn, Mail, Lock, ArrowLeft, Loader2, Eye, EyeOff } from "lucide-react";
import { useState } from "react";

export async function clientAction({ request }: Route.ActionArgs) {
  const formData = await request.formData();
  try {
    await sendBuyerLoginRequest(formData);
    return redirect("/buyer");
  } catch {
    alert("Invalid email or password");
  }
}

export default function BuyerLoginPage() {
  const navigation = useNavigation();
  const isLoggingIn = navigation.state === "submitting";
  const [showPassword, setShowPassword] = useState(false);

  return (
    <div className="min-h-screen w-full flex flex-col items-center justify-center bg-slate-50 p-4">
      {/* Back to Home Link */}
      <Link 
        to="/" 
        className="absolute top-8 left-8 flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-primary transition-colors"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Home
      </Link>

      <Card className="w-full max-w-md border-2 shadow-xl">
        <CardHeader className="space-y-1 text-center">
          <div className="flex justify-center mb-2">
            <div className="p-3 rounded-full bg-primary/10">
              <LogIn className="h-6 w-6 text-primary" />
            </div>
          </div>
          <CardTitle className="text-2xl font-bold tracking-tight">Buyer Login</CardTitle>
          <CardDescription>
            Enter your credentials to access your account
          </CardDescription>
        </CardHeader>

        <CardContent>
          <Form method="post" className="space-y-4">
            <Field className="space-y-2">
              <FieldLabel className="text-sm font-semibold">Email Address</FieldLabel>
              <div className="relative">
                <Mail className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input 
                  name="email" 
                  placeholder="name@example.com" 
                  type="email" 
                  required 
                  className="pl-10"
                  disabled={isLoggingIn}
                />
              </div>
            </Field>

            <Field className="space-y-2">
              <FieldLabel className="text-sm font-semibold">Password</FieldLabel>
              <div className="relative">
                <Lock className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input 
                  name="password" 
                  placeholder="••••••••" 
                  type={showPassword ? "text" : "password"} 
                  required 
                  className="pl-10"
                  disabled={isLoggingIn}
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute right-3 top-3 text-muted-foreground hover:text-foreground"
                >
                  {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              </div>
            </Field>

            <Button 
              type="submit" 
              className="w-full h-11 font-bold mt-2" 
              disabled={isLoggingIn}
            >
              {isLoggingIn ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Signing in...
                </>
              ) : (
                "Sign In"
              )}
            </Button>
          </Form>

          <div className="mt-6 text-center text-sm">
            <span className="text-muted-foreground">Don't have an account? </span>
            <Link 
              to="/auth/buyer/register" 
              className="font-bold text-primary hover:underline"
            >
              Create an account
            </Link>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

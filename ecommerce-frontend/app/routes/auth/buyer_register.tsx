import { useState, useEffect } from "react";
import { Form, redirect, useNavigation, Link } from "react-router";
import { Button } from "~/components/ui/button";
import { Field, FieldLabel } from "~/components/ui/field";
import { Input } from "~/components/ui/input";
import type { Route } from "./+types/buyer_register";
import { sendBuyerRegisterRequest } from "~/services/AuthService";
import axios from "axios";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "~/components/ui/card";
import { 
  UserPlus, 
  Mail, 
  Lock, 
  User, 
  MapPin, 
  ArrowLeft, 
  Loader2, 
  Eye, 
  EyeOff 
} from "lucide-react";
import { toast } from "sonner";

export async function clientAction({ request }: Route.ActionArgs) {
  const formData = await request.formData();
  try {
    await sendBuyerRegisterRequest(formData);
    return redirect("/auth/buyer/login");
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 409) {
        return { error: "This email is already registered." };
      }
      if (error.response?.status === 400) {
        return { error: "Please check your input and try again." };
      }
    }
    return { error: "Registration failed. Please try again later." };
  }
}

export default function BuyerRegisterPage({ actionData }: Route.ComponentProps) {
  const navigation = useNavigation();
  const isRegistering = navigation.state === "submitting";
  const [showPassword, setShowPassword] = useState(false);

  useEffect(() => {
    if (actionData?.error) {
      toast.error(actionData.error);
    }
  }, [actionData]);

  return (
    <div className="min-h-screen w-full flex flex-col items-center justify-center bg-slate-50 p-4 py-12">
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
              <UserPlus className="h-6 w-6 text-primary" />
            </div>
          </div>
          <CardTitle className="text-2xl font-bold tracking-tight">Create Account</CardTitle>
          <CardDescription>
            Join us today and start shopping
          </CardDescription>
        </CardHeader>

        <CardContent>
          <Form method="post" className="space-y-4">
            {/* Name Field */}
            <Field className="space-y-2">
              <FieldLabel className="text-sm font-semibold">Full Name</FieldLabel>
              <div className="relative">
                <User className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input 
                  name="name" 
                  placeholder="John Doe" 
                  type="text" 
                  required 
                  className="pl-10"
                  disabled={isRegistering}
                />
              </div>
            </Field>

            {/* Email Field */}
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
                  disabled={isRegistering}
                />
              </div>
            </Field>

            {/* Password Field */}
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
                  disabled={isRegistering}
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

            {/* Address Field */}
            <Field className="space-y-2">
              <FieldLabel className="text-sm font-semibold">Delivery Address</FieldLabel>
              <div className="relative">
                <MapPin className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
                <Input 
                  name="address" 
                  placeholder="123 Street, City, Country" 
                  type="text" 
                  required 
                  className="pl-10"
                  disabled={isRegistering}
                />
              </div>
            </Field>

            <Button 
              type="submit" 
              className="w-full h-11 font-bold mt-4" 
              disabled={isRegistering}
            >
              {isRegistering ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Creating account...
                </>
              ) : (
                "Register Now"
              )}
            </Button>
          </Form>

          <div className="mt-6 text-center text-sm">
            <span className="text-muted-foreground">Already have an account? </span>
            <Link 
              to="/auth/buyer/login" 
              className="font-bold text-primary hover:underline"
            >
              Sign in
            </Link>
          </div>
        </CardContent>
      </Card>

      <p className="mt-8 text-center text-xs text-muted-foreground max-w-xs leading-relaxed">
        By registering, you agree to receive updates and special offers via the email provided.
      </p>
    </div>
  );
}
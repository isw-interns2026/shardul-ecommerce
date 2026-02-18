import { Link, useSearchParams } from "react-router";
import { Card, CardContent } from "~/components/ui/card";
import { Button } from "~/components/ui/button";
import { CheckCircle2, Package, ShoppingBag, PartyPopper } from "lucide-react";

export default function OrderSuccessPage() {
  const [searchParams] = useSearchParams();
  const sessionId = searchParams.get("session_id");

  return (
    <div className="mx-auto max-w-lg px-6 pt-16 animate-fade-in">
      <Card className="text-center overflow-hidden border-2">
        {/* Celebration banner */}
        <div className="relative bg-linear-to-br from-green-50 to-emerald-50 py-10 overflow-hidden">
          {/* Decorative dots */}
          <div
            className="absolute top-3 left-6 w-2 h-2 rounded-full bg-green-300 animate-bounce"
            style={{ animationDelay: "0ms" }}
          />
          <div
            className="absolute top-8 right-10 w-1.5 h-1.5 rounded-full bg-emerald-400 animate-bounce"
            style={{ animationDelay: "300ms" }}
          />
          <div
            className="absolute bottom-4 left-14 w-2.5 h-2.5 rounded-full bg-green-200 animate-bounce"
            style={{ animationDelay: "150ms" }}
          />
          <div
            className="absolute top-5 left-1/3 w-1.5 h-1.5 rounded-full bg-primary/30 animate-bounce"
            style={{ animationDelay: "450ms" }}
          />
          <div
            className="absolute bottom-6 right-16 w-2 h-2 rounded-full bg-emerald-300 animate-bounce"
            style={{ animationDelay: "200ms" }}
          />

          <div className="relative animate-scale-in">
            <CheckCircle2 className="h-16 w-16 text-green-600 mx-auto drop-shadow-md" />
          </div>
        </div>

        <CardContent className="p-8 space-y-4">
          <div className="flex items-center justify-center gap-2 text-primary">
            <PartyPopper className="h-5 w-5" />
          </div>

          <h1 className="text-2xl font-bold tracking-tight">
            Order Placed Successfully!
          </h1>
          <p className="text-muted-foreground leading-relaxed">
            Thank you for your purchase. Your payment has been processed and
            your order is being prepared for shipping.
          </p>

          {sessionId && (
            <p className="text-xs text-muted-foreground font-mono bg-muted/50 rounded-md px-3 py-2 inline-block">
              Session: {sessionId.slice(0, 20)}…
            </p>
          )}

          <div className="flex flex-col sm:flex-row gap-3 pt-4">
            <Button asChild size="lg" className="flex-1 font-bold">
              <Link to="/buyer/orders">
                <Package className="mr-2 h-4 w-4" />
                View Orders
              </Link>
            </Button>
            <Button
              asChild
              variant="outline"
              size="lg"
              className="flex-1 font-semibold"
            >
              <Link to="/buyer">
                <ShoppingBag className="mr-2 h-4 w-4" />
                Continue Shopping
              </Link>
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

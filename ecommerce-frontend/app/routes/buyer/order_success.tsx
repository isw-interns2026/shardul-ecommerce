import { Link, useSearchParams } from "react-router";
import { Card, CardContent } from "~/components/ui/card";
import { Button } from "~/components/ui/button";
import { CheckCircle2, Package, ShoppingBag } from "lucide-react";

export default function OrderSuccessPage() {
  const [searchParams] = useSearchParams();
  const sessionId = searchParams.get("session_id");

  return (
    <div className="mx-auto max-w-lg px-6 pt-16">
      <Card className="text-center overflow-hidden">
        <div className="bg-green-50 py-8">
          <CheckCircle2 className="h-16 w-16 text-green-600 mx-auto" />
        </div>

        <CardContent className="p-8 space-y-4">
          <h1 className="text-2xl font-bold tracking-tight">
            Order Placed Successfully!
          </h1>
          <p className="text-muted-foreground">
            Thank you for your purchase. Your payment has been processed and your
            order is being prepared.
          </p>

          {sessionId && (
            <p className="text-xs text-muted-foreground font-mono bg-muted/50 rounded-md px-3 py-2 inline-block">
              Session: {sessionId.slice(0, 20)}…
            </p>
          )}

          <div className="flex flex-col sm:flex-row gap-3 pt-4">
            <Button asChild className="flex-1">
              <Link to="/buyer/orders">
                <Package className="mr-2 h-4 w-4" />
                View Orders
              </Link>
            </Button>
            <Button asChild variant="outline" className="flex-1">
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

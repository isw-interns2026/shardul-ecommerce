/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import apiClient from "~/axios_instance";
import axios from "axios";
import type { BuyerCartItemResponseDto } from "~/types/ResponseDto";
import type { Route } from "./+types/view_cart";
import { useFetcher, useFetchers } from "react-router";
import { Card, CardContent, CardFooter } from "~/components/ui/card";
import { Button } from "~/components/ui/button";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "~/components/ui/alert-dialog";
import {
  CreditCard,
  Loader2,
  Minus,
  Plus,
  ShoppingBag,
  Trash2,
} from "lucide-react";
import { toast } from "sonner";
import { useEffect } from "react";
import { useCart } from "~/context/CartContext";

export async function clientLoader() {
  try {
    const response = await apiClient.get(`/buyer/cart`);
    const cartItems: BuyerCartItemResponseDto[] = response.data;
    return { cartItems };
  } catch {
    toast.error("Failed to load cart. Please refresh the page.");
    return { cartItems: [] };
  }
}

export async function clientAction({ request }: Route.ActionArgs) {
  const formData = await request.formData();
  const productId = formData.get("productId") as string;
  const count = Number(formData.get("count"));

  try {
    await apiClient.post(`/buyer/cart/${productId}?count=${count}`);
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 422) {
      toast.error("Not enough stock available.");
    } else {
      toast.error("Failed to update cart. Please try again.");
    }
  }
  return null;
}

export default function CartDisplay({ loaderData }: Route.ComponentProps) {
  const { cartItems } = loaderData;
  const fetchers = useFetchers();
  const placeOrderFetcher = useFetcher();
  const isPlacingOrder = placeOrderFetcher.state !== "idle";
  const { refreshCart } = useCart();

  // Refresh the navbar cart badge whenever fetchers settle back to idle
  const anyActive = fetchers.some((f) => f.state !== "idle");
  useEffect(() => {
    if (!anyActive) {
      void refreshCart();
    }
  }, [anyActive, refreshCart]);

  // 1. Identify items being deleted across all active fetchers
  const deletingIds = new Set(
    fetchers
      .filter((f) => f.state !== "idle" && f.formAction?.includes("delete"))
      .map((f) => f.formAction?.split("/").pop()),
  );

  // 2. Identify quantity updates across all active fetchers
  const quantityUpdates = new Map(
    fetchers
      .filter((f) => f.state !== "idle" && f.formData?.has("count"))
      .map((f) => [
        f.formData?.get("productId"),
        Number(f.formData?.get("count")),
      ]),
  );

  // 3. Create the Optimistic Cart List
  const optimisticCartItems = cartItems
    .filter((item) => !deletingIds.has(item.id))
    .map((item) => {
      const pendingCount = quantityUpdates.get(item.id);
      return pendingCount !== undefined
        ? { ...item, countInCart: pendingCount }
        : item;
    });

  // 4. Calculate Total based on optimistic data
  const cartTotal = optimisticCartItems.reduce(
    (acc, item) => acc + item.price * item.countInCart,
    0,
  );

  if (optimisticCartItems.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-24 text-center animate-fade-in">
        <div className="p-5 rounded-2xl bg-primary/10 mb-6">
          <ShoppingBag className="h-10 w-10 text-primary" />
        </div>
        <h2 className="text-2xl font-bold tracking-tight mb-2">
          Your cart is empty
        </h2>
        <p className="text-muted-foreground max-w-sm mb-6">
          Looks like you haven&apos;t added anything yet.
        </p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl p-6 space-y-6 animate-fade-in">
      <div className="flex items-end justify-between">
        <div>
          <h1 className="text-2xl font-bold">Shopping Cart</h1>
          <p className="text-sm text-muted-foreground mt-1">
            {optimisticCartItems.length} item
            {optimisticCartItems.length !== 1 && "s"}
          </p>
        </div>
      </div>

      <div className="space-y-4">
        {optimisticCartItems.map((item) => (
          <CartItem key={item.id} item={item} />
        ))}
      </div>

      <Card className="bg-muted/30 border-2">
        <CardContent className="p-6 space-y-3">
          <div className="flex justify-between text-sm text-muted-foreground">
            <span>
              Subtotal (
              {optimisticCartItems.reduce((acc, i) => acc + i.countInCart, 0)}{" "}
              items)
            </span>
            <span>₹{cartTotal.toFixed(2)}</span>
          </div>
          <div className="flex justify-between text-sm text-muted-foreground">
            <span>Shipping</span>
            <span className="text-green-600 font-medium">Free</span>
          </div>
          <div className="border-t pt-3 flex justify-between text-lg font-bold">
            <span>Order Total</span>
            <span className="text-primary">₹{cartTotal.toFixed(2)}</span>
          </div>
        </CardContent>
        <CardFooter className="p-6 pt-0">
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button
                size="lg"
                className="w-full text-base font-bold"
                disabled={isPlacingOrder}
              >
                {isPlacingOrder ? (
                  <>
                    <Loader2 className="mr-2 h-5 w-5 animate-spin" />
                    Processing...
                  </>
                ) : (
                  <>
                    <CreditCard className="mr-2 h-5 w-5" />
                    Place Order
                  </>
                )}
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Confirm your order</AlertDialogTitle>
                <AlertDialogDescription>
                  You are about to place an order for{" "}
                  <span className="font-semibold text-foreground">
                    ₹{cartTotal.toFixed(2)}
                  </span>
                  . You will be redirected to Stripe to complete payment.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Cancel</AlertDialogCancel>
                <AlertDialogAction
                  onClick={() =>
                    placeOrderFetcher.submit(null, {
                      action: "place_order",
                      method: "post",
                    })
                  }
                >
                  Confirm &amp; Pay
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        </CardFooter>
      </Card>
    </div>
  );
}

function CartItem({ item }: { item: BuyerCartItemResponseDto }) {
  const fetcher = useFetcher();
  const lineTotal = item.price * item.countInCart;

  return (
    <Card className="overflow-hidden hover:border-primary/20 transition-colors">
      <CardContent className="flex items-center justify-between py-4 gap-4">
        <div className="space-y-1 flex-1 min-w-0">
          <div className="font-semibold truncate">{item.name}</div>
          <div className="text-sm text-muted-foreground flex items-center gap-2">
            <span>₹{item.price.toFixed(2)} each</span>
            <span className="text-muted-foreground/50">&middot;</span>
            <span className="font-semibold text-foreground">
              ₹{lineTotal.toFixed(2)}
            </span>
          </div>
        </div>

        <div className="flex items-center gap-6">
          <div className="flex items-center gap-2">
            <fetcher.Form method="post">
              <input type="hidden" name="productId" value={item.id} />
              <input
                type="hidden"
                name="count"
                value={Math.max(item.countInCart - 1, 1)}
              />
              <Button
                variant="outline"
                size="icon"
                className="h-8 w-8"
                disabled={item.countInCart <= 1}
                type="submit"
              >
                <Minus className="h-3 w-3" />
              </Button>
            </fetcher.Form>

            <span className="w-8 text-center font-medium">
              {item.countInCart}
            </span>

            <fetcher.Form method="post">
              <input type="hidden" name="productId" value={item.id} />
              <input type="hidden" name="count" value={item.countInCart + 1} />
              <Button
                variant="outline"
                size="icon"
                className="h-8 w-8"
                type="submit"
                disabled={item.countInCart >= item.availableStock}
              >
                <Plus className="h-3 w-3" />
              </Button>
            </fetcher.Form>
          </div>

          <fetcher.Form method="post" action={`delete/${item.id}`}>
            <Button
              variant="ghost"
              size="icon"
              className="text-destructive hover:bg-destructive/10"
              type="submit"
            >
              <Trash2 className="h-5 w-5" />
            </Button>
          </fetcher.Form>
        </div>
      </CardContent>
    </Card>
  );
}

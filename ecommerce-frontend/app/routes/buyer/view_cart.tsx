/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import apiClient from "~/axios_instance";
import axios from "axios";
import type { BuyerCartItemResponseDto } from "~/types/ResponseDto";
import type { Route } from "./+types/view_cart";
import { useFetcher, useFetchers } from "react-router";
import { Card, CardContent, CardFooter } from "~/components/ui/card";
import { Button } from "~/components/ui/button";
import { CreditCard, Loader2, Minus, Plus, ShoppingBag, Trash2 } from "lucide-react";
import { toast } from "sonner";

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

  // 1. Identify items being deleted across all active fetchers
  const deletingIds = new Set(
    fetchers
      .filter((f) => f.state !== "idle" && f.formAction?.includes("delete"))
      .map((f) => f.formAction?.split("/").pop())
  );

  // 2. Identify quantity updates across all active fetchers
  const quantityUpdates = new Map(
    fetchers
      .filter((f) => f.state !== "idle" && f.formData?.has("count"))
      .map((f) => [f.formData?.get("productId"), Number(f.formData?.get("count"))])
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
    0
  );

  if (optimisticCartItems.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center p-12 text-muted-foreground">
        <ShoppingBag className="h-12 w-12 mb-4 opacity-20" />
        <p className="text-lg font-medium">Your cart is empty</p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-3xl p-6 space-y-6">
      <h1 className="text-2xl font-bold">Shopping Cart</h1>
      
      <div className="space-y-4">
        {optimisticCartItems.map((item) => (
          <CartItem key={item.id} item={item} />
        ))}
      </div>

      <Card className="bg-muted/30">
        <CardContent className="p-6 space-y-4">
          <div className="flex justify-between text-lg font-semibold">
            <span>Order Total</span>
            <span>₹{cartTotal.toFixed(2)}</span>
          </div>
        </CardContent>
        <CardFooter className="p-6 pt-0">
          <placeOrderFetcher.Form action="place_order" method="post" className="w-full">
            <Button size="lg" className="w-full text-base font-bold" disabled={isPlacingOrder}>
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
          </placeOrderFetcher.Form>
        </CardFooter>
      </Card>
    </div>
  );
}

function CartItem({ item }: { item: BuyerCartItemResponseDto }) {
  const fetcher = useFetcher();

  return (
    <Card>
      <CardContent className="flex items-center justify-between py-4">
        <div className="space-y-1">
          <div className="font-semibold">{item.name}</div>
          <div className="text-sm text-muted-foreground">
            ₹{item.price.toFixed(2)}
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
              <input
                type="hidden"
                name="count"
                value={item.countInCart + 1}
              />
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






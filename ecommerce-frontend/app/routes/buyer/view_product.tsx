/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import { useState } from "react";
import type { BuyerProductResponseDto } from "~/types/ResponseDto";
import type { Route } from "./+types/view_product";
import apiClient from "~/axios_instance";
import axios from "axios";
import { useFetcher } from "react-router";
import { Button } from "~/components/ui/button";
import { Card, CardContent, CardHeader } from "~/components/ui/card";
import { Minus, Plus, ShoppingCart, Check, PackageX } from "lucide-react";
import { toast } from "sonner";

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const productId: string = params.productId;
  try {
    const response = await apiClient.get(`/buyer/products/${productId}`);
    const productDto: BuyerProductResponseDto = response.data;
    return { productDto, notFound: false };
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 404) {
      return { productDto: null, notFound: true };
    }
    toast.error("Failed to load product. Please try again.");
    return { productDto: null, notFound: false };
  }
}

export async function clientAction({
  params,
  request,
}: Route.ClientActionArgs) {
  const formData = await request.formData();
  const quantity = formData.get("quantity");
  const productId = params.productId;

  try {
    await apiClient.post(`/buyer/cart/${productId}?count=${quantity}`);
    return { success: true, addedQuantity: Number(quantity) };
  } catch {
    toast.error("Failed to add to cart. Please try again.");
    return { success: false };
  }
}

export default function ProductDisplay({ loaderData }: Route.ComponentProps) {
  const { productDto, notFound } = loaderData;
  const fetcher = useFetcher();
  const [quantity, setQuantity] = useState(1);

  if (notFound) {
    return (
      <div className="flex flex-col items-center justify-center p-12 text-muted-foreground">
        <PackageX className="h-12 w-12 mb-4 opacity-20" />
        <p className="text-lg font-medium">Product not found</p>
      </div>
    );
  }

  if (!productDto) {
    return (
      <div className="flex flex-col items-center justify-center p-12 text-muted-foreground">
        <p className="text-lg font-medium">Failed to load product</p>
      </div>
    );
  }

  const isSubmitting = fetcher.state !== "idle";
  const isSuccess =
    fetcher.data?.success && quantity === fetcher.data?.addedQuantity;

  const handleIncrement = () => {
    if (quantity < productDto.availableStock) {
      setQuantity((prev) => prev + 1);
    }
  };

  const handleDecrement = () => {
    if (quantity > 1) {
      setQuantity((prev) => prev - 1);
    }
  };

  return (
    <div className="mx-auto max-w-6xl px-6 pt-6">
      <Card className="grid grid-cols-1 md:grid-cols-2 gap-6 overflow-hidden">
        {/* Image Section */}
        <div className="bg-muted/50 rounded-l-lg h-100 md:h-full overflow-hidden">
          {productDto.imageUrl ? (
            <img
              src={productDto.imageUrl}
              alt={productDto.name}
              className="h-full w-full object-cover"
            />
          ) : (
            <div className="flex h-full items-center justify-center">
              <span className="text-muted-foreground">No image available</span>
            </div>
          )}
        </div>

        <div className="flex flex-col gap-6 p-6">
          <CardHeader className="p-0">
            <div className="space-y-1">
              <h1 className="text-3xl font-bold tracking-tight">
                {productDto.name}
              </h1>
            </div>
          </CardHeader>

          <CardContent className="p-0 flex flex-col gap-6">
            <div className="flex items-baseline gap-2">
              {/* Price - Using Rupee Symbol */}
              <span className="text-4xl font-bold text-primary">
                ₹{productDto.price.toFixed(2)}
              </span>
            </div>

            <div className="space-y-2">
              <p className="text-sm font-medium">
                {productDto.availableStock > 0 ? (
                  <span className="text-green-600 flex items-center gap-1">
                    <span className="relative flex h-2 w-2">
                      <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
                      <span className="relative inline-flex rounded-full h-2 w-2 bg-green-500"></span>
                    </span>
                    {productDto.availableStock} items in stock
                  </span>
                ) : (
                  <span className="text-red-600">Out of stock</span>
                )}
              </p>
              {productDto.description && (
                <p className="text-muted-foreground leading-relaxed whitespace-pre-wrap wrap-break-word">
                  {productDto.description}
                </p>
              )}
            </div>

            {productDto.availableStock > 0 && (
              <div className="space-y-4 pt-4 border-t">
                <label className="text-sm font-semibold">Quantity</label>
                <div className="flex items-center gap-4">
                  <div className="flex items-center border rounded-lg p-1 bg-background">
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8"
                      onClick={handleDecrement}
                      disabled={quantity <= 1 || isSubmitting}
                    >
                      <Minus className="h-4 w-4" />
                    </Button>
                    <span className="w-10 text-center font-medium">
                      {quantity}
                    </span>
                    <Button
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8"
                      onClick={handleIncrement}
                      disabled={
                        quantity >= productDto.availableStock || isSubmitting
                      }
                    >
                      <Plus className="h-4 w-4" />
                    </Button>
                  </div>

                  <fetcher.Form method="post" className="flex-1">
                    <input type="hidden" name="quantity" value={quantity} />
                    <Button
                      type="submit"
                      className="w-full transition-all"
                      disabled={isSubmitting || isSuccess}
                      variant={isSuccess ? "outline" : "default"}
                    >
                      {isSubmitting ? (
                        "Adding..."
                      ) : isSuccess ? (
                        <>
                          <Check className="mr-2 h-4 w-4 text-green-600" />{" "}
                          Added to Cart
                        </>
                      ) : (
                        <>
                          <ShoppingCart className="mr-2 h-4 w-4" /> Add to Cart
                        </>
                      )}
                    </Button>
                  </fetcher.Form>
                </div>
              </div>
            )}
          </CardContent>
        </div>
      </Card>
    </div>
  );
}

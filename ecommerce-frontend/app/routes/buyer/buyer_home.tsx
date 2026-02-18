/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import apiClient from "~/axios_instance";
import type { BuyerProductResponseDto } from "~/types/ResponseDto";
import type { Route } from "./+types/buyer_home";
import { Link } from "react-router";
import { Card, CardContent } from "~/components/ui/card";
import { Badge } from "~/components/ui/badge";
import { Package, ShoppingBag, ArrowRight } from "lucide-react";
import { Button } from "~/components/ui/button";
import { toast } from "sonner";

export async function clientLoader() {
  try {
    const response = await apiClient.get("/buyer/products");
    const products: BuyerProductResponseDto[] = response.data;

    const sortedProducts = [...products].sort((a, b) => {
      const aStock = a.availableStock > 0 ? 1 : 0;
      const bStock = b.availableStock > 0 ? 1 : 0;
      return bStock - aStock;
    });

    return { products: sortedProducts };
  } catch {
    toast.error("Failed to load products. Please refresh the page.");
    return { products: [] };
  }
}

export default function BuyerHomePage({ loaderData }: Route.ComponentProps) {
  const { products } = loaderData;

  if (!products.length) {
    return (
      <div className="mx-auto max-w-7xl p-6">
        <div className="flex flex-col items-center justify-center py-24 text-center animate-fade-in">
          <div className="p-5 rounded-2xl bg-primary/10 mb-6">
            <ShoppingBag className="h-10 w-10 text-primary" />
          </div>
          <h2 className="text-2xl font-bold tracking-tight mb-2">
            No products available
          </h2>
          <p className="text-muted-foreground max-w-sm">
            New products are being added all the time. Check back soon!
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl p-6">
      {/* Header */}
      <div className="flex items-end justify-between mb-8 animate-slide-up">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Explore Products
          </h1>
          <p className="text-muted-foreground mt-1">
            {products.length} product{products.length !== 1 && "s"} available
          </p>
        </div>
      </div>

      {/* Product Grid */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6 stagger-children">
        {products.map((product) => (
          <Link
            key={product.id}
            to={`/buyer/products/${product.id}`}
            className="group outline-none animate-scale-in"
          >
            <Card className="h-full overflow-hidden border transition-all duration-300 hover:border-primary/40 hover:shadow-xl hover:-translate-y-1">
              {/* Image */}
              <div className="aspect-square bg-linear-to-br from-muted/30 to-muted/80 overflow-hidden relative">
                {product.imageUrl ? (
                  <img
                    src={product.imageUrl}
                    alt={product.name}
                    className="h-full w-full object-cover transition-transform duration-500 group-hover:scale-110"
                  />
                ) : (
                  <div className="flex h-full items-center justify-center">
                    <Package className="h-12 w-12 text-muted-foreground/30" />
                  </div>
                )}

                {/* Stock overlay for out-of-stock */}
                {product.availableStock <= 0 && (
                  <div className="absolute inset-0 bg-background/60 backdrop-blur-[2px] flex items-center justify-center">
                    <Badge
                      variant="destructive"
                      className="text-xs font-bold uppercase"
                    >
                      Out of Stock
                    </Badge>
                  </div>
                )}
              </div>

              <CardContent className="p-4 space-y-3">
                <div className="flex items-start justify-between gap-2">
                  <h3 className="font-bold text-base leading-tight line-clamp-2 group-hover:text-primary transition-colors">
                    {product.name}
                  </h3>
                  <ArrowRight className="h-4 w-4 shrink-0 text-muted-foreground/0 group-hover:text-primary transition-all mt-0.5 -translate-x-2 group-hover:translate-x-0 opacity-0 group-hover:opacity-100" />
                </div>

                {product.description && (
                  <p className="text-sm text-muted-foreground line-clamp-2 min-h-10">
                    {product.description}
                  </p>
                )}

                <div className="pt-2 flex items-center justify-between border-t">
                  <div className="text-xl font-black text-primary">
                    ₹{product.price.toFixed(2)}
                  </div>

                  {product.availableStock > 0 && (
                    <Badge
                      variant="secondary"
                      className="text-[10px] font-bold uppercase tracking-wider"
                    >
                      In Stock
                    </Badge>
                  )}
                </div>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  );
}

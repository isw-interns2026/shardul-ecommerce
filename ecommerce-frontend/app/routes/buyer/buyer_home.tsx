/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import apiClient from "~/axios_instance";
import type { BuyerProductResponseDto } from "~/types/ResponseDto";
import type { Route } from "./+types/buyer_home";
import { Link } from "react-router";
import { Card, CardContent } from "~/components/ui/card";

export async function clientLoader() {
  const response = await apiClient.get("/buyer/products");
  const products: BuyerProductResponseDto[] = response.data;
  
  const sortedProducts = [...products].sort((a, b) => {
    const aStock = a.availableStock > 0 ? 1 : 0;
    const bStock = b.availableStock > 0 ? 1 : 0;
    return bStock - aStock; // 1 (in stock) comes before 0 (out of stock)
  });

  return { products: sortedProducts };
}

export default function BuyerHomePage({ loaderData }: Route.ComponentProps) {
  const { products } = loaderData;

  return (
    <div className="mx-auto max-w-7xl p-6">
      <h1 className="text-3xl font-bold mb-8 tracking-tight">
        Explore Products
      </h1>

      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {products.map((product) => (
          <Link
            key={product.id}
            to={`/buyer/products/${product.id}`}
            className="group outline-none"
          >
            <Card className="h-full overflow-hidden border-2 transition-all duration-300 hover:border-primary/50 hover:shadow-xl">
              <div className="aspect-square bg-muted/50 overflow-hidden border-b">
                {product.imageUrl ? (
                  <img
                    src={product.imageUrl}
                    alt={product.name}
                    className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                  />
                ) : (
                  <div className="flex h-full items-center justify-center">
                    <span className="text-xs text-muted-foreground">
                      No Image
                    </span>
                  </div>
                )}
              </div>

              <CardContent className="p-4 space-y-3">
                <h3 className="font-bold text-lg leading-tight truncate group-hover:text-primary transition-colors">
                  {product.name}
                </h3>

                {product.description && (
                  <p className="text-sm text-muted-foreground line-clamp-2 min-h-10">
                    {product.description}
                  </p>
                )}

                <div className="pt-2 flex items-center justify-between">
                  <div className="text-xl font-black text-primary">
                    ₹{product.price.toFixed(2)}
                  </div>

                  <div className="text-[10px] font-bold uppercase tracking-widest">
                    {product.availableStock > 0 ? (
                      <span className="text-green-600 bg-green-50 px-2 py-1 rounded">
                        In Stock
                      </span>
                    ) : (
                      <span className="text-red-600 bg-red-50 px-2 py-1 rounded">
                        Out of Stock
                      </span>
                    )}
                  </div>
                </div>
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  );
}

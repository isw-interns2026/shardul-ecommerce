import apiClient from "~/axios_instance";
import type { BuyerProductResponseDto } from "~/types/ResponseDto";
import type { Route } from "./+types/buyer_home";
import { Link } from "react-router";
import { Card, CardContent, CardHeader, CardTitle } from "~/components/ui/card";

export async function clientLoader() {
  const response = await apiClient.get("/buyer/products");
  const products: BuyerProductResponseDto[] = response.data; //eslint-disable-line
  // console.log(products);
  return { products };
}

export default function BuyerHomePage({ loaderData }: Route.ComponentProps) {
  const { products } = loaderData;

  return (
    <div className="h-full overflow-y-auto p-6">
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
        {products.map((product) => (
          <Link
            key={product.id}
            to={`/buyer/products/${product.id}`}
            className="focus:outline-none"
          >
            <Card className="cursor-pointer hover:shadow-lg transition-shadow">
              <CardHeader>
                <CardTitle className="truncate">{product.name}</CardTitle>
              </CardHeader>

              <CardContent className="space-y-2">
                <div className="text-sm text-muted-foreground">
                  SKU: {product.sku}
                </div>

                <div className="font-semibold">₹{product.price}</div>

                <div className="text-sm">In stock: {product.countInStock}</div>

                {product.description && (
                  <p className="text-sm text-muted-foreground line-clamp-2">
                    {product.description}
                  </p>
                )}
              </CardContent>
            </Card>
          </Link>
        ))}
      </div>
    </div>
  );
}

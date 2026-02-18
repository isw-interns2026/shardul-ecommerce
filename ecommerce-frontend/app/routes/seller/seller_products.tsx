/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import { useState } from "react";
import { Link, useRevalidator } from "react-router";
import apiClient from "~/axios_instance";
import type { SellerProductResponseDto } from "~/types/ResponseDto";
import type { Route } from "./+types/seller_products";
import { Button } from "~/components/ui/button";
import { Badge } from "~/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "~/components/ui/table";
import { Switch } from "~/components/ui/switch";
import { Plus, Package, Pencil, Loader2 } from "lucide-react";
import { toast } from "sonner";

export async function clientLoader() {
  try {
    const response = await apiClient.get("/seller/products");
    const products: SellerProductResponseDto[] = response.data;
    return { products };
  } catch {
    toast.error("Failed to load products. Please refresh the page.");
    return { products: [] };
  }
}

export default function SellerProductsPage({
  loaderData,
}: Route.ComponentProps) {
  const { products } = loaderData;
  const revalidator = useRevalidator();

  if (!products.length) {
    return (
      <div className="mx-auto max-w-7xl p-6">
        <div className="flex items-center justify-between mb-8">
          <h1 className="text-3xl font-bold tracking-tight">My Products</h1>
          <Button asChild>
            <Link to="/seller/products/new">
              <Plus className="mr-2 h-4 w-4" />
              Add Product
            </Link>
          </Button>
        </div>
        <div className="flex flex-col items-center justify-center p-12 text-muted-foreground">
          <Package className="h-12 w-12 mb-4 opacity-20" />
          <p className="text-lg font-medium">No products yet</p>
          <p className="text-sm mt-1">Add your first product to get started.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl p-6">
      <div className="flex items-center justify-between mb-8">
        <h1 className="text-3xl font-bold tracking-tight">My Products</h1>
        <Button asChild>
          <Link to="/seller/products/new">
            <Plus className="mr-2 h-4 w-4" />
            Add Product
          </Link>
        </Button>
      </div>

      <div className="rounded-lg border shadow-sm">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>SKU</TableHead>
              <TableHead className="text-right">Price</TableHead>
              <TableHead className="text-right">Stock</TableHead>
              <TableHead className="text-center">Listed</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {products.map((product) => (
              <ProductRow
                key={product.id}
                product={product}
                onToggle={() => revalidator.revalidate()}
              />
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}

function ProductRow({
  product,
  onToggle,
}: {
  product: SellerProductResponseDto;
  onToggle: () => void;
}) {
  const [isToggling, setIsToggling] = useState(false);

  const handleToggleListed = async () => {
    setIsToggling(true);
    try {
      await apiClient.patch(`/seller/products/${product.id}`, {
        isListed: !product.isListed,
      });
      toast.success(product.isListed ? "Product unlisted" : "Product listed");
      onToggle();
    } catch {
      toast.error("Failed to update listing status.");
    } finally {
      setIsToggling(false);
    }
  };

  return (
    <TableRow>
      <TableCell className="font-medium max-w-[200px] truncate">
        {product.name}
      </TableCell>
      <TableCell>
        <code className="text-xs bg-muted px-1.5 py-0.5 rounded">
          {product.sku}
        </code>
      </TableCell>
      <TableCell className="text-right font-semibold">
        ₹{product.price.toFixed(2)}
      </TableCell>
      <TableCell className="text-right">
        {product.countInStock > 0 ? (
          <Badge variant="secondary" className="font-mono">
            {product.countInStock}
          </Badge>
        ) : (
          <Badge variant="destructive" className="font-mono">
            0
          </Badge>
        )}
      </TableCell>
      <TableCell className="text-center">
        {isToggling ? (
          <Loader2 className="h-4 w-4 animate-spin mx-auto" />
        ) : (
          <Switch
            checked={product.isListed}
            onCheckedChange={handleToggleListed}
            aria-label={`Toggle listing for ${product.name}`}
          />
        )}
      </TableCell>
      <TableCell className="text-right">
        <Button variant="ghost" size="sm" asChild>
          <Link to={`/seller/products/${product.id}`}>
            <Pencil className="mr-1 h-3.5 w-3.5" />
            Edit
          </Link>
        </Button>
      </TableCell>
    </TableRow>
  );
}

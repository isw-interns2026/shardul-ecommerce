/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import { useState, useEffect } from "react";
import { Link, useNavigation, useRevalidator } from "react-router";
import apiClient from "~/axios_instance";
import axios from "axios";
import type { SellerProductResponseDto } from "~/types/ResponseDto";
import type { Route } from "./+types/edit_product";
import { Button } from "~/components/ui/button";
import { Field, FieldLabel } from "~/components/ui/field";
import { Input } from "~/components/ui/input";
import { Textarea } from "~/components/ui/textarea";
import { Switch } from "~/components/ui/switch";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "~/components/ui/card";
import {
  ArrowLeft,
  Loader2,
  PackageX,
  Pencil,
  Save,
  Package,
  ImageIcon,
} from "lucide-react";
import { toast } from "sonner";

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const productId: string = params.productId;
  try {
    const response = await apiClient.get(`/seller/products/${productId}`);
    const product: SellerProductResponseDto = response.data;
    return { product, notFound: false };
  } catch (error) {
    if (axios.isAxiosError(error) && error.response?.status === 404) {
      return { product: null, notFound: true };
    }
    toast.error("Failed to load product. Please try again.");
    return { product: null, notFound: false };
  }
}

export default function EditProductPage({ loaderData }: Route.ComponentProps) {
  const { product, notFound } = loaderData;

  if (notFound) {
    return (
      <div className="flex flex-col items-center justify-center p-12 text-muted-foreground">
        <PackageX className="h-12 w-12 mb-4 opacity-20" />
        <p className="text-lg font-medium">Product not found</p>
      </div>
    );
  }

  if (!product) {
    return (
      <div className="flex flex-col items-center justify-center p-12 text-muted-foreground">
        <p className="text-lg font-medium">Failed to load product</p>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl p-6 animate-fade-in">
      <Link
        to="/seller"
        className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-primary transition-colors mb-6"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Products
      </Link>

      <div className="space-y-6 stagger-children">
        <StockCard product={product} />
        <EditDetailsCard product={product} />
      </div>
    </div>
  );
}

function StockCard({ product }: { product: SellerProductResponseDto }) {
  const [stock, setStock] = useState(String(product.countInStock));
  const [isSaving, setIsSaving] = useState(false);
  const revalidator = useRevalidator();

  const handleUpdateStock = async (e: React.FormEvent) => {
    e.preventDefault();
    const newStock = Number(stock);
    if (isNaN(newStock) || newStock < 0) {
      toast.error("Stock must be a non-negative number.");
      return;
    }
    setIsSaving(true);
    try {
      await apiClient.put(`/seller/products/${product.id}/stock`, {
        countInStock: newStock,
      });
      toast.success("Stock updated successfully!");
      revalidator.revalidate();
    } catch {
      toast.error("Failed to update stock.");
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Card className="border-2">
      <CardHeader className="pb-3">
        <div className="flex items-center gap-3">
          <div className="p-2 rounded-lg bg-primary/10">
            <Package className="h-5 w-5 text-primary" />
          </div>
          <div>
            <CardTitle className="text-lg font-bold">
              Stock Management
            </CardTitle>
            <CardDescription>Quickly update inventory count</CardDescription>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        <form onSubmit={handleUpdateStock} className="flex items-end gap-3">
          <Field className="space-y-2 flex-1">
            <FieldLabel className="text-sm font-semibold">
              Stock Count
            </FieldLabel>
            <Input
              type="number"
              min="0"
              value={stock}
              onChange={(e) => setStock(e.target.value)}
              disabled={isSaving}
            />
          </Field>
          <Button type="submit" disabled={isSaving} className="h-10">
            {isSaving ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <>
                <Save className="mr-1 h-4 w-4" />
                Update Stock
              </>
            )}
          </Button>
        </form>
      </CardContent>
    </Card>
  );
}

function EditDetailsCard({ product }: { product: SellerProductResponseDto }) {
  const navigation = useNavigation();
  const isSubmitting = navigation.state === "submitting";
  const [isListed, setIsListed] = useState(product.isListed);
  const [actionError, setActionError] = useState<string | null>(null);
  const [imageUrl, setImageUrl] = useState(product.imageUrl ?? "");
  const revalidator = useRevalidator();

  const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);

    const body: Record<string, unknown> = {};
    const name = formData.get("name") as string;
    const price = Number(formData.get("price"));
    const description = (formData.get("description") as string) || null;
    const imageUrl = (formData.get("imageUrl") as string) || null;

    if (name !== product.name) body.name = name;
    if (price !== product.price) body.price = price;
    if (description !== product.description) body.description = description;
    if (imageUrl !== product.imageUrl) body.imageUrl = imageUrl;
    if (isListed !== product.isListed) body.isListed = isListed;

    if (Object.keys(body).length === 0) {
      toast.info("No changes to save.");
      return;
    }

    try {
      await apiClient.patch(`/seller/products/${product.id}`, body);
      toast.success("Product updated successfully!");
      setActionError(null);
      revalidator.revalidate();
    } catch (error) {
      if (axios.isAxiosError(error)) {
        if (error.response?.status === 400) {
          setActionError("Please check your input and try again.");
        } else {
          setActionError("Failed to update product. Please try again.");
        }
      } else {
        setActionError("An unexpected error occurred.");
      }
    }
  };

  useEffect(() => {
    if (actionError) {
      toast.error(actionError);
    }
  }, [actionError]);

  return (
    <Card className="border-2 shadow-lg">
      <CardHeader className="space-y-1">
        <div className="flex items-center gap-3">
          <div className="p-2 rounded-lg bg-primary/10">
            <Pencil className="h-5 w-5 text-primary" />
          </div>
          <div>
            <CardTitle className="text-2xl font-bold tracking-tight">
              Edit Product
            </CardTitle>
            <CardDescription>
              Update product details — SKU:{" "}
              <code className="text-xs bg-muted px-1.5 py-0.5 rounded">
                {product.sku}
              </code>
            </CardDescription>
          </div>
        </div>
      </CardHeader>

      <CardContent>
        <form onSubmit={handleSubmit} className="space-y-5">
          <Field className="space-y-2">
            <FieldLabel className="text-sm font-semibold">
              Product Name
            </FieldLabel>
            <Input
              name="name"
              defaultValue={product.name}
              type="text"
              required
              disabled={isSubmitting}
            />
          </Field>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <Field className="space-y-2">
              <FieldLabel className="text-sm font-semibold">
                Price (₹)
              </FieldLabel>
              <Input
                name="price"
                defaultValue={product.price}
                type="number"
                step="0.01"
                min="0.01"
                required
                disabled={isSubmitting}
              />
            </Field>
          </div>

          <Field className="space-y-2">
            <FieldLabel className="text-sm font-semibold">
              Description (optional)
            </FieldLabel>
            <Textarea
              name="description"
              defaultValue={product.description ?? ""}
              rows={3}
              disabled={isSubmitting}
            />
          </Field>

          <Field className="space-y-2">
            <FieldLabel className="text-sm font-semibold">
              Image URL (optional)
            </FieldLabel>
            <Input
              name="imageUrl"
              defaultValue={product.imageUrl ?? ""}
              type="url"
              disabled={isSubmitting}
              onChange={(e) => setImageUrl(e.target.value)}
            />
            {/* Image Preview */}
            {imageUrl && (
              <div className="mt-2 rounded-lg border overflow-hidden bg-muted/30 aspect-video max-w-xs">
                <img
                  src={imageUrl}
                  alt="Product preview"
                  className="h-full w-full object-cover"
                  onError={(e) => {
                    e.currentTarget.style.display = "none";
                    e.currentTarget.nextElementSibling?.classList.remove(
                      "hidden",
                    );
                  }}
                  onLoad={(e) => {
                    e.currentTarget.style.display = "block";
                    e.currentTarget.nextElementSibling?.classList.add("hidden");
                  }}
                />
                <div className="items-center justify-center h-full text-muted-foreground text-sm gap-2 hidden">
                  <ImageIcon className="h-4 w-4" />
                  Invalid image URL
                </div>
              </div>
            )}
          </Field>

          <div className="flex items-center gap-3 pt-2">
            <Switch
              id="editIsListed"
              checked={isListed}
              onCheckedChange={setIsListed}
              disabled={isSubmitting}
            />
            <label
              htmlFor="editIsListed"
              className="text-sm font-semibold cursor-pointer"
            >
              Product is listed
            </label>
          </div>

          <div className="flex gap-3 pt-4">
            <Button
              type="submit"
              className="flex-1 h-11 font-bold"
              disabled={isSubmitting}
            >
              {isSubmitting ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Saving...
                </>
              ) : (
                "Save Changes"
              )}
            </Button>
            <Button
              type="button"
              variant="outline"
              className="h-11"
              asChild
              disabled={isSubmitting}
            >
              <Link to="/seller">Cancel</Link>
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  );
}

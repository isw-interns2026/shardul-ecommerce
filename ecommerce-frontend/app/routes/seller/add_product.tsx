import { useState, useEffect } from "react";
import { Form, redirect, useNavigation, Link } from "react-router";
import { Button } from "~/components/ui/button";
import { Field, FieldLabel } from "~/components/ui/field";
import { Input } from "~/components/ui/input";
import { Textarea } from "~/components/ui/textarea";
import { Switch } from "~/components/ui/switch";
import type { Route } from "./+types/add_product";
import apiClient from "~/axios_instance";
import axios from "axios";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "~/components/ui/card";
import { PackagePlus, ArrowLeft, Loader2 } from "lucide-react";
import { toast } from "sonner";

export async function clientAction({ request }: Route.ActionArgs) {
  const formData = await request.formData();

  const body = {
    sku: formData.get("sku"),
    name: formData.get("name"),
    price: Number(formData.get("price")),
    countInStock: Number(formData.get("countInStock")),
    description: formData.get("description") || null,
    imageUrl: formData.get("imageUrl") || null,
    isListed: formData.get("isListed") === "true",
  };

  try {
    await apiClient.post("/seller/products", body);
    toast.success("Product created successfully!");
    return redirect("/seller");
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 409) {
        return { error: "A product with this SKU already exists." };
      }
      if (error.response?.status === 400) {
        return { error: "Please check your input and try again." };
      }
    }
    return { error: "Failed to create product. Please try again later." };
  }
}

export default function AddProductPage({ actionData }: Route.ComponentProps) {
  const navigation = useNavigation();
  const isSubmitting = navigation.state === "submitting";
  const [isListed, setIsListed] = useState(true);

  useEffect(() => {
    if (actionData?.error) {
      toast.error(actionData.error);
    }
  }, [actionData]);

  return (
    <div className="mx-auto max-w-2xl p-6">
      <Link
        to="/seller"
        className="inline-flex items-center gap-2 text-sm font-medium text-muted-foreground hover:text-primary transition-colors mb-6"
      >
        <ArrowLeft className="h-4 w-4" />
        Back to Products
      </Link>

      <Card className="border-2 shadow-lg">
        <CardHeader className="space-y-1">
          <div className="flex items-center gap-3">
            <div className="p-2 rounded-lg bg-primary/10">
              <PackagePlus className="h-5 w-5 text-primary" />
            </div>
            <div>
              <CardTitle className="text-2xl font-bold tracking-tight">
                Add Product
              </CardTitle>
              <CardDescription>Create a new product listing</CardDescription>
            </div>
          </div>
        </CardHeader>

        <CardContent>
          <Form method="post" className="space-y-5">
            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <Field className="space-y-2">
                <FieldLabel className="text-sm font-semibold">SKU</FieldLabel>
                <Input
                  name="sku"
                  placeholder="PROD-001"
                  type="text"
                  required
                  disabled={isSubmitting}
                />
              </Field>

              <Field className="space-y-2">
                <FieldLabel className="text-sm font-semibold">
                  Product Name
                </FieldLabel>
                <Input
                  name="name"
                  placeholder="Product name"
                  type="text"
                  required
                  disabled={isSubmitting}
                />
              </Field>
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
              <Field className="space-y-2">
                <FieldLabel className="text-sm font-semibold">
                  Price (₹)
                </FieldLabel>
                <Input
                  name="price"
                  placeholder="99.99"
                  type="number"
                  step="0.01"
                  min="0.01"
                  required
                  disabled={isSubmitting}
                />
              </Field>

              <Field className="space-y-2">
                <FieldLabel className="text-sm font-semibold">
                  Stock Count
                </FieldLabel>
                <Input
                  name="countInStock"
                  placeholder="100"
                  type="number"
                  min="0"
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
                placeholder="Describe your product..."
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
                placeholder="https://example.com/image.jpg"
                type="url"
                disabled={isSubmitting}
              />
            </Field>

            <div className="flex items-center gap-3 pt-2">
              <Switch
                id="isListed"
                checked={isListed}
                onCheckedChange={setIsListed}
                disabled={isSubmitting}
              />
              <label
                htmlFor="isListed"
                className="text-sm font-semibold cursor-pointer"
              >
                List product immediately
              </label>
              <input type="hidden" name="isListed" value={String(isListed)} />
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
                    Creating...
                  </>
                ) : (
                  "Create Product"
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
          </Form>
        </CardContent>
      </Card>
    </div>
  );
}

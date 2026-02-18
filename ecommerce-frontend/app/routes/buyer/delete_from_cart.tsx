/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import apiClient from "~/axios_instance";
import invariant from "tiny-invariant";
import type { Route } from "./+types/delete_from_cart";
import { toast } from "sonner";

export async function clientAction({ params }: Route.ClientActionArgs) {
  invariant(params.productId, "Missing productId parameter");
  try {
    await apiClient.delete(`/buyer/cart/${params.productId}`);
  } catch {
    toast.error("Failed to remove item from cart.");
  }
  return null;
}

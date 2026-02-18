import apiClient from "~/axios_instance";
import axios from "axios";
import { toast } from "sonner";

export async function clientAction() {
  try {
    const response = await apiClient.post(`buyer/cart`);
    const { checkoutUrl } = response.data as { checkoutUrl: string };

    // Redirect to Stripe's hosted checkout page
    window.location.href = checkoutUrl;
  } catch (error) {
    if (axios.isAxiosError(error)) {
      if (error.response?.status === 400) {
        toast.error("Your cart is empty.");
      } else if (error.response?.status === 422) {
        toast.error("Insufficient stock for one or more items. Please update your cart.");
      } else {
        toast.error("Failed to place order. Please try again.");
      }
    } else {
      toast.error("Something went wrong. Please try again.");
    }
  }

  // Return null — either redirect happened or error toast was shown
  return null;
}

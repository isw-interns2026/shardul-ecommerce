import {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from "react";
import apiClient from "~/axios_instance";
import type { BuyerCartItemResponseDto } from "~/types/ResponseDto";

interface CartContextType {
  cartCount: number;
  refreshCart: () => Promise<void>;
}

const CartContext = createContext<CartContextType>({
  cartCount: 0,
  refreshCart: async () => {},
});

export function CartProvider({ children }: { children: ReactNode }) {
  const [cartCount, setCartCount] = useState(0);

  const refreshCart = useCallback(async () => {
    try {
      const token = localStorage.getItem("accessToken");
      if (!token) {
        setCartCount(0);
        return;
      }
      const response = await apiClient.get("/buyer/cart");
      const cartItems: BuyerCartItemResponseDto[] = response.data;
      const totalItems = cartItems.reduce(
        (sum, item) => sum + item.countInCart,
        0,
      );
      setCartCount(totalItems);
    } catch {
      // Silently fail — don't disrupt UX for a badge
      setCartCount(0);
    }
  }, []);

  useEffect(() => {
    void refreshCart();
  }, [refreshCart]);

  return (
    <CartContext.Provider value={{ cartCount, refreshCart }}>
      {children}
    </CartContext.Provider>
  );
}

export function useCart() {
  return useContext(CartContext);
}

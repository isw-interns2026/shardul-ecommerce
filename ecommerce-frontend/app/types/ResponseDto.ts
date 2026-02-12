export const enum UserRole {
  BUYER = "Buyer",
  SELLER = "Seller",
}

export interface BuyerProductResponseDto {
  id: string;
  sku: string;
  name: string;
  price: number;
  availableStock: number;
  description: string | null;
  imageUrl: string | null;
}

export interface BuyerCartItemResponseDto {
  id: string;
  sku: string;
  name: string;
  price: number;
  availableStock: number;
  description: string | null;
  imageUrl: string | null;
  countInCart: number;
}

export type OrderStatus =
  | "Delivered"
  | "WaitingForSellerToAccept"
  | "InTransit"
  | "AwaitingPayment"
  | "Cancelled";

export interface BuyerOrderResponseDto {
  orderId: string;
  orderValue: number;
  productCount: number;
  productId: string;
  productName: string;
  productSku: string;
  deliveryAddress: string;
  orderStatus: OrderStatus;
}

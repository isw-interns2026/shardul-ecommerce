export const enum UserRole {
  BUYER = "Buyer",
  SELLER = "Seller",
}

export interface BuyerProductResponseDto {
  id: string;
  sku: string;
  name: string;
  price: number;
  countInStock: number;
  description: string | null;
  images: string | null;
}

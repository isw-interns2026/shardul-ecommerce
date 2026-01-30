/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import type { BuyerProductResponseDto } from "~/types/ResponseDto";
import type { Route } from "./+types/view_product";
import apiClient from "~/axios_instance";

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const productId: string = params.productId;
  const response = await apiClient.get(`/buyer/products/${productId}`);
  const productDto: BuyerProductResponseDto = response.data;
  //   console.log(productDto);
  return { productDto };
}

export async function action(){
    // TODO: Add to cart button
}

export default function ProductDisplay({ loaderData }: Route.ComponentProps) {
  const { productDto } = loaderData;
  return (
    <>
      <div>Here we are...</div>
      <div>{productDto.id}</div>
    </>
  );
}

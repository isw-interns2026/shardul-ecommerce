/* eslint-disable @typescript-eslint/no-unsafe-assignment */
import apiClient from "~/axios_instance";

export async function clientLoader() {
  const response = await apiClient.get(`/buyer/cart`);

  //   console.log(productDto);
  return { productDto };
}

export async function action(){
    // TODO: Add to cart button
}

export default function CartDisplay({ loaderData }: Route.ComponentProps) {
  const { cartDto } = loaderData;
  return (
    <>
      <div>Here we are...</div>
      <div>{productDto.id}</div>
    </>
  );
}

import apiClient from "~/axios_instance";

export async function clientLoader(){
  const products = await apiClient.get("/buyer/products");
  console.log(products);
  return { products };
}


export default function BuyerHomePage() {
  return (
    <>
      <div>Well here we are...</div>
      <div>Suffering...</div>
    </>
  );
}

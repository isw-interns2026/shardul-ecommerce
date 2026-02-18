import apiClient from "~/axios_instance";

function formDataToObject(formData: FormData): Record<string, string> {
  return Object.fromEntries(formData.entries()) as Record<string, string>;
}

export async function sendBuyerLoginRequest(formData: FormData) {
  const response = await apiClient.post(
    "/auth/buyer/login",
    formDataToObject(formData),
  );
  localStorage.setItem("accessToken", response.data.accessToken); //eslint-disable-line
}

export async function sendBuyerRegisterRequest(formData: FormData) {
  await apiClient.post("/auth/buyer/register", formDataToObject(formData));
}

export async function sendSellerLoginRequest(formData: FormData) {
  const response = await apiClient.post(
    "/auth/seller/login",
    formDataToObject(formData),
  );
  localStorage.setItem("accessToken", response.data.accessToken);
}

export async function sendSellerRegisterRequest(formData: FormData) {
  await apiClient.post("/auth/seller/register", formDataToObject(formData));
}

import apiClient from "~/axios_instance";

export async function sendBuyerLoginRequest(formData: FormData) {
  const response = await apiClient.post("/auth/buyer/login", formData);
  localStorage.setItem("accessToken", response.data.accessToken); //eslint-disable-line
}

export async function sendBuyerRegisterRequest(formData: FormData) {
  await apiClient.post("/auth/buyer/register", formData);
}

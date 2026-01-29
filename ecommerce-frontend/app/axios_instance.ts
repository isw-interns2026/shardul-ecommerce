import axios from "axios";

const apiClient = axios.create({
  baseURL: "http://localhost:5191/",
  timeout: 5000,
  headers: {
    "content-type": "application/json",
  },
});

apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem("accessToken");
    if (token){
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) =>{
    // eslint-disable-next-line @typescript-eslint/prefer-promise-reject-errors
    return Promise.reject(error);
  }
)

export default apiClient;

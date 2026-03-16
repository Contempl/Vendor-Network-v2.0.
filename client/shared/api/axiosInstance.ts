import { TokenDto } from "@/entities/auth/auth-types";
import axios from "axios";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5227"; 


export const apiClient = axios.create({
    baseURL: API_BASE_URL,
    headers: {
        'Content-Type': 'application/json',
    },
});



apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("tkn-tko");
  if (token)
    {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

apiClient.interceptors.response.use(
    (response) => response, 
    async (error) => {
       const status = error.response?.status
       if (status === 401)
       {
          try {
            const refreshToken = localStorage.getItem("refreshToken");
            const response = await apiClient.post<TokenDto>("/auth/refresh", { refreshToken });
            localStorage.setItem("tkn-tko", response.data.accessToken);
            localStorage.setItem("refreshToken", response.data.refreshToken);
            return apiClient(error.config);
          }
            catch (refreshError) {
                console.error("Token refresh failed", refreshError);
                return Promise.reject(error);
            }
       }
       return Promise.reject(error);
    }
);
import { TokenDto } from "@/entities/auth/auth-types";
import { clearAuthTokens } from "@/entities/auth/auth-utils";
import axios from "axios";
import type { AxiosError, InternalAxiosRequestConfig } from "axios";

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5227"; 

let refreshPromise: Promise<TokenDto> | null = null;

export const refreshAccessToken = async (): Promise<TokenDto> => {
  if (refreshPromise) return refreshPromise;
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) throw new Error("No refresh token is available");

  refreshPromise = axios.post<TokenDto>(`${API_BASE_URL}/refresh`, { refreshToken })
    .then(({ data }) => {
      localStorage.setItem("tkn-tko", data.accessToken);
      localStorage.setItem("refreshToken", data.refreshToken);
      return data;
    })
    .finally(() => { refreshPromise = null; });

  return refreshPromise;
};

type RetriableRequest = InternalAxiosRequestConfig & { _retry?: boolean };


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
    async (error: AxiosError) => {
       const request = error.config as RetriableRequest | undefined;
       const isLogin = request?.url === "/Account/Login" || request?.url === "/Admin/Login";
       if (error.response?.status !== 401 || !request || request._retry || isLogin || !localStorage.getItem("refreshToken")) {
         return Promise.reject(error);
       }

       request._retry = true;
       try {
         const tokens = await refreshAccessToken();
         request.headers.Authorization = `Bearer ${tokens.accessToken}`;
         return apiClient(request);
       } catch {
         clearAuthTokens();
         window.location.assign(window.location.pathname.startsWith("/admin") ? "/admin/login" : "/login");
         return Promise.reject(error);
       }
    }
);

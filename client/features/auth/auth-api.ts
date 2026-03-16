import { TokenDto } from "@/entities/auth/auth-types";
import { apiClient } from "@/shared/api/axiosInstance";

export async function login(email: string, password: string) {
    const response = await apiClient.post<TokenDto>("/Account/Login", { email, password });
    return response.data;
}
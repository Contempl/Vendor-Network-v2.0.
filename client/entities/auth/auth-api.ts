import { InviteIdToFrontEnd, TokenDto, UserRegistrationByInviteDto } from "@/entities/auth/auth-types";
import { apiClient } from "@/shared/api/axiosInstance";

export async function login(email: string, password: string) {
    const response = await apiClient.post<TokenDto>("/Account/Login", { email, password });
    return response.data;
}

export async function getInvite(inviteId: number) {
    const response = await apiClient.get<InviteIdToFrontEnd>(`/Account/Register/User/${inviteId}`);
    return response.data;
}

export async function registerByInvite(inviteId: number, data: UserRegistrationByInviteDto) {
    const response = await apiClient.post(`/Account/Register/User/${inviteId}`, data);
    return response.data;
}
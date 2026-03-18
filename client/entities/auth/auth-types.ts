export interface TokenDto {
    accessToken: string;
    refreshToken: string;
}

export interface TokenPayload {
    userId: string;
    email: string;
    role: string;
    businessId: string;
}
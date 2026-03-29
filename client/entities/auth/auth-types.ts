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

export interface InviteIdToFrontEnd {
    inviteId: number;
}

export interface UserRegistrationByInviteDto {
    userName: string;
    firstName: string;
    lastName: string;
    password: string;
}
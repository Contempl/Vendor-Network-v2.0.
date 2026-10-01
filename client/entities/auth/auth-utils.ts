import { jwtDecode } from "jwt-decode";
import { TokenPayload } from "./auth-types";

export type UserRole = "Admin" | "VendorUser" | "OperatorUser";

const roleClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

type AccessTokenPayload = TokenPayload & {
    exp?: number;
    [roleClaim]?: string;
};

export const clearAuthTokens = () => {
    localStorage.removeItem("tkn-tko");
    localStorage.removeItem("refreshToken");
};

export const getTokenPayload = (token: string): AccessTokenPayload | null => {
    try {
        return jwtDecode<AccessTokenPayload>(token);
    } catch {
        return null;
    }
};

export const getRoleFromToken = (token: string): UserRole | null => {
    const payload = getTokenPayload(token);
    const role = payload?.[roleClaim] ?? payload?.role;
    return role === "Admin" || role === "VendorUser" || role === "OperatorUser" ? role : null;
};

export const getHomeForRole = (role: UserRole) =>
    role === "Admin" ? "/admin/dashboard" : role === "OperatorUser" ? "/operator" : "/vendor";

export const isTokenExpired = (token: string) => {
    const expiresAt = getTokenPayload(token)?.exp;
    return !expiresAt || expiresAt * 1000 <= Date.now();
};

export const getBusinessIdFromToken = () => {
    const token = localStorage.getItem("tkn-tko");
    if (!token) return null;
    return getTokenPayload(token)?.businessId ?? null;
};

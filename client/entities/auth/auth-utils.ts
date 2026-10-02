import { jwtDecode } from "jwt-decode";
import { TokenPayload } from "./auth-types";

export type UserRole = "Admin" | "SuperAdmin" | "VendorUser" | "OperatorUser";

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
    return role === "Admin" || role === "SuperAdmin" || role === "VendorUser" || role === "OperatorUser" ? role : null;
};

export const getBusinessTypeFromToken = (token: string): "Vendor" | "Operator" | null => {
    const type = getTokenPayload(token)?.businessType;
    return type === "Vendor" || type === "Operator" ? type : null;
};

export const getHomeForRole = (role: UserRole, token: string) => {
    if (role === "SuperAdmin") return "/admin/dashboard";
    if (role === "OperatorUser") return "/operator";
    if (role === "VendorUser") return "/vendor";
    const businessType = getBusinessTypeFromToken(token);
    return businessType === "Operator" ? "/operator" : businessType === "Vendor" ? "/vendor" : "/login";
};

export const isTokenExpired = (token: string) => {
    const expiresAt = getTokenPayload(token)?.exp;
    return !expiresAt || expiresAt * 1000 <= Date.now();
};

export const getBusinessIdFromToken = () => {
    const token = localStorage.getItem("tkn-tko");
    if (!token) return null;
    return getTokenPayload(token)?.businessId ?? null;
};

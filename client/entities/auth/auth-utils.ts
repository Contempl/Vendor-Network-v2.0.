import { jwtDecode } from "jwt-decode";
import { TokenPayload } from "./auth-types";

export const getBusinessIdFromToken = () => {
    const token = localStorage.getItem("tkn-tko");
    if (!token) return null;
    const payload = jwtDecode<TokenPayload>(token);
    return payload.businessId;
};
"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Box, CircularProgress } from "@mui/material";
import { refreshAccessToken } from "@/shared/api/axiosInstance";
import { clearAuthTokens, getBusinessTypeFromToken, getHomeForRole, getRoleFromToken, isTokenExpired, UserRole } from "./auth-utils";

export default function ProtectedRoute({
  allowedRole,
  businessType,
  children,
}: {
  allowedRole: UserRole | UserRole[];
  businessType?: "Vendor" | "Operator";
  children: React.ReactNode;
}) {
  const router = useRouter();
  const [authorized, setAuthorized] = useState(false);
  const adminRoute = allowedRole === "SuperAdmin" || (Array.isArray(allowedRole) && allowedRole.includes("SuperAdmin"));

  useEffect(() => {
    let active = true;

    const checkSession = async () => {
      let token = localStorage.getItem("tkn-tko");
      if (!token && !localStorage.getItem("refreshToken")) {
        router.replace(adminRoute ? "/admin/login" : "/login");
        return;
      }

      try {
        if (!token || isTokenExpired(token)) {
          token = (await refreshAccessToken()).accessToken;
        }
        const role = getRoleFromToken(token);
        if (!role) throw new Error("Invalid token role");
        if (!active) return;
        if (!(Array.isArray(allowedRole) ? allowedRole.includes(role) : role === allowedRole) ||
            (businessType && role === "Admin" && getBusinessTypeFromToken(token) !== businessType)) {
          router.replace(getHomeForRole(role, token));
        } else {
          setAuthorized(true);
        }
      } catch {
        clearAuthTokens();
        if (active) router.replace(adminRoute ? "/admin/login" : "/login");
      }
    };

    void checkSession();
    return () => { active = false; };
  }, [allowedRole, adminRoute, businessType, router]);

  if (!authorized) {
    return <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "100vh", bgcolor: "#0d0d0d" }}>
      <CircularProgress sx={{ color: "#e94560" }} />
    </Box>;
  }

  return <>{children}</>;
}

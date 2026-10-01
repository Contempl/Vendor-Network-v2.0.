"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Box, CircularProgress } from "@mui/material";
import { refreshAccessToken } from "@/shared/api/axiosInstance";
import { clearAuthTokens, getHomeForRole, getRoleFromToken, isTokenExpired, UserRole } from "./auth-utils";

export default function ProtectedRoute({
  allowedRole,
  children,
}: {
  allowedRole: UserRole;
  children: React.ReactNode;
}) {
  const router = useRouter();
  const [authorized, setAuthorized] = useState(false);

  useEffect(() => {
    let active = true;

    const checkSession = async () => {
      let token = localStorage.getItem("tkn-tko");
      if (!token && !localStorage.getItem("refreshToken")) {
        router.replace(allowedRole === "Admin" ? "/admin/login" : "/login");
        return;
      }

      try {
        if (!token || isTokenExpired(token)) {
          token = (await refreshAccessToken()).accessToken;
        }
        const role = getRoleFromToken(token);
        if (!role) throw new Error("Invalid token role");
        if (!active) return;
        if (role !== allowedRole) {
          router.replace(getHomeForRole(role));
        } else {
          setAuthorized(true);
        }
      } catch {
        clearAuthTokens();
        if (active) router.replace(allowedRole === "Admin" ? "/admin/login" : "/login");
      }
    };

    void checkSession();
    return () => { active = false; };
  }, [allowedRole, router]);

  if (!authorized) {
    return <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "100vh", bgcolor: "#0d0d0d" }}>
      <CircularProgress sx={{ color: "#e94560" }} />
    </Box>;
  }

  return <>{children}</>;
}

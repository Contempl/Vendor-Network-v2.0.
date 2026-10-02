"use client";

import { usePathname } from "next/navigation";
import ProtectedRoute from "@/entities/auth/ProtectedRoute";

export default function AdminLayout({ children }: { children: React.ReactNode }) {
    const pathname = usePathname();
    if (pathname === "/admin/login") return <>{children}</>;
    return <ProtectedRoute allowedRole="SuperAdmin">{children}</ProtectedRoute>;
}

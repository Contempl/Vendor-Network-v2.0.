"use client";

import { AppRouterCacheProvider } from "@mui/material-nextjs/v15-appRouter";
import { ReactNode } from "react";

export default function MuiProvider({ children }: { children: ReactNode }) {
    return (
        <AppRouterCacheProvider>
            {children}
        </AppRouterCacheProvider>
    );
}
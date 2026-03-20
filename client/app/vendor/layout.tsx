"use client";

import {
  Box, Drawer, List, ListItemButton, ListItemIcon,
  ListItemText, Typography, Avatar, Divider
} from "@mui/material";
import BusinessIcon from "@mui/icons-material/Business";
import ApartmentIcon from "@mui/icons-material/Apartment";
import SearchIcon from "@mui/icons-material/Search";
import PersonAddIcon from "@mui/icons-material/PersonAdd";
import LogoutIcon from "@mui/icons-material/Logout";
import { useRouter, usePathname } from "next/navigation";

const DRAWER_WIDTH = 260;

const navItems = [
  { label: "Профиль", icon: <BusinessIcon />, path: "/vendor" },
  { label: "Facilities", icon: <ApartmentIcon />, path: "/vendor/facilities" },
  { label: "Поиск операторов", icon: <SearchIcon />, path: "/vendor/operators" },
  { label: "Пригласить", icon: <PersonAddIcon />, path: "/vendor/invite" },
];

export default function VendorLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();

  const handleLogout = () => {
    localStorage.removeItem("tkn-tko");
    localStorage.removeItem("refreshToken");
    router.push("/login");
  }

  return (
    <Box sx={{ display: "flex", minHeight: "100vh", bgcolor: "#0d0d0d" }}>
      <Drawer
        variant="permanent"
        sx={{
          width: DRAWER_WIDTH,
          "& .MuiDrawer-paper": {
            width: DRAWER_WIDTH,
            bgcolor: "#111111",
            border: "none",
            borderRight: "1px solid rgba(255,255,255,0.06)",
          },
        }}
      >
        {/* Logo */}
        <Box sx={{ p: 3, display: "flex", alignItems: "center", gap: 1.5 }}>
          <Avatar sx={{ bgcolor: "#e94560", width: 36, height: 36 }}>
            <BusinessIcon fontSize="small" />
          </Avatar>
          <Typography fontWeight={800} color="white" letterSpacing="-0.5px">
            VendorApp
          </Typography>
        </Box>

        <Divider sx={{ borderColor: "rgba(255,255,255,0.06)" }} />

        {/* Nav Items */}
        <List sx={{ px: 1.5, pt: 2, flexGrow: 1 }}>
          {navItems.map((item) => {
            const isActive = pathname === item.path;
            return (
              <ListItemButton
                key={item.path}
                onClick={() => router.push(item.path)}
                sx={{
                  borderRadius: 2,
                  mb: 0.5,
                  bgcolor: isActive ? "rgba(233,69,96,0.12)" : "transparent",
                  "&:hover": { bgcolor: "rgba(255,255,255,0.05)" },
                }}
              >
                <ListItemIcon sx={{ color: isActive ? "#e94560" : "rgba(255,255,255,0.4)", minWidth: 36 }}>
                  {item.icon}
                </ListItemIcon>
                <ListItemText
                  primary={item.label}
                  primaryTypographyProps={{
                    fontSize: 14,
                    fontWeight: isActive ? 700 : 400,
                    color: isActive ? "white" : "rgba(255,255,255,0.5)",
                  }}
                />
              </ListItemButton>
            );
          })}
        </List>

        <Divider sx={{ borderColor: "rgba(255,255,255,0.06)" }} />

        {/* Logout */}
        <List sx={{ px: 1.5, py: 1.5 }}>
          <ListItemButton
            onClick={handleLogout}
            sx={{ borderRadius: 2, "&:hover": { bgcolor: "rgba(233,69,96,0.08)" } }}
          >
            <ListItemIcon sx={{ color: "rgba(255,255,255,0.3)", minWidth: 36 }}>
              <LogoutIcon />
            </ListItemIcon>
            <ListItemText
              primary="Выйти"
              primaryTypographyProps={{ fontSize: 14, color: "rgba(255,255,255,0.3)" }}
            />
          </ListItemButton>
        </List>
      </Drawer>

      {/* Page Content */}
      <Box component="main" sx={{ flexGrow: 1, overflow: "auto" }}>
        {children}
      </Box>
    </Box>
  );
}
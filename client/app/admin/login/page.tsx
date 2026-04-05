"use client";

import {
  Box, Button, Container, TextField, Typography, Paper, Alert
} from "@mui/material";
import LockOutlinedIcon from "@mui/icons-material/LockOutlined";
import { useState } from "react";
import { login } from "@/entities/auth/auth-api";
import { useRouter } from "next/navigation";

export default function LoginPage() {
    const router = useRouter();
    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (_e: React.MouseEvent<HTMLButtonElement>) => {
        try {
            const response = await login(email, password);
            localStorage.setItem("tkn-tko", response.accessToken);
            localStorage.setItem("refreshToken", response.refreshToken);
            router.push("/admin/dashboard");
        } catch (err) {
            setError("Неверный email или пароль");
        }
    };

  return (
    <Box sx={{
      minHeight: "100vh",
      background: "linear-gradient(135deg, #1a1a2e 0%, #16213e 50%, #0f3460 100%)",
      display: "flex",
      alignItems: "center",
    }}>
      <Container maxWidth="xs">
        <Paper elevation={0} sx={{
          p: 4,
          borderRadius: 3,
          background: "rgba(255,255,255,0.05)",
          backdropFilter: "blur(20px)",
          border: "1px solid rgba(255,255,255,0.1)",
        }}>
          <Box sx={{ display: "flex", flexDirection: "column", alignItems: "center", mb: 3 }}>
            <Box sx={{
              bgcolor: "#e94560",
              borderRadius: "50%",
              p: 1.5,
              mb: 2
            }}>
              <LockOutlinedIcon sx={{ color: "white" }} />
            </Box>
            <Typography variant="h5" fontWeight={700} color="white">
              Войти в систему
            </Typography>
          </Box>

          
          {error && <Alert severity="error">{error}</Alert>}


          <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <TextField
              label="Email"
              type="email"
              fullWidth
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              sx={{
                "& .MuiOutlinedInput-root": {
                  color: "white",
                  "& fieldset": { borderColor: "rgba(255,255,255,0.2)" },
                  "&:hover fieldset": { borderColor: "#e94560" },
                },
                "& .MuiInputLabel-root": { color: "rgba(255,255,255,0.5)" },
              }}
            />
            <TextField
              label="Пароль"
              type="password"
              fullWidth
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              sx={{
                "& .MuiOutlinedInput-root": {
                  color: "white",
                  "& fieldset": { borderColor: "rgba(255,255,255,0.2)" },
                  "&:hover fieldset": { borderColor: "#e94560" },
                },
                "& .MuiInputLabel-root": { color: "rgba(255,255,255,0.5)" },
              }}
            />
            <Button
              variant="contained"
              fullWidth
              onClick={handleSubmit}
              sx={{
                mt: 1,
                py: 1.5,
                bgcolor: "#e94560",
                "&:hover": { bgcolor: "#c73652" },
                fontWeight: 700,
                borderRadius: 2,
              }}
            >
              Войти
            </Button>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
}
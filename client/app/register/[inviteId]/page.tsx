"use client";

import {
  Box, Button, Container, TextField, Typography, Paper, Alert
} from "@mui/material";
import HowToRegIcon from "@mui/icons-material/HowToReg";
import { useEffect, useState } from "react";
import { getInvite, registerByInvite } from "@/entities/auth/auth-api";
import React from "react";
import { useRouter } from "next/navigation";


export default function RegisterPage({ params }: { params: Promise<{ inviteId: string }> }) {
  const { inviteId } = React.use(params);
  const router = useRouter();

  const [userName, setUserName] = useState<string>("");
  const [firstName, setFirstName] = useState<string>("");
  const [lastName, setLastName] = useState<string>("");
  const [password, setPassword] = useState<string>("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const checkInvite = async () => {
      try {
        await getInvite(Number(inviteId));
      } catch {
        router.push("/login");
      }
    };
    checkInvite();
  }, []);

  const handleRegister = async () => {
    setLoading(true);
    setError(null);
    try {
      await registerByInvite(Number(inviteId), { userName, firstName, lastName, password });
      router.push("/login");
    } catch {
      setError("Не удалось зарегистрироваться. Проверьте данные и попробуйте снова.");
    } finally {
      setLoading(false);
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
            <Box sx={{ bgcolor: "#e94560", borderRadius: "50%", p: 1.5, mb: 2 }}>
              <HowToRegIcon sx={{ color: "white" }} />
            </Box>
            <Typography variant="h5" fontWeight={700} color="white">
              Регистрация
            </Typography>
            <Typography color="rgba(255,255,255,0.4)" fontSize={13} mt={0.5}>
              Заполните данные для создания аккаунта
            </Typography>
          </Box>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }} onClose={() => setError(null)}>
              {error}
            </Alert>
          )}

          <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            {[
              { label: "Имя пользователя", value: userName, setter: setUserName, type: "text" },
              { label: "Имя", value: firstName, setter: setFirstName, type: "text" },
              { label: "Фамилия", value: lastName, setter: setLastName, type: "text" },
              { label: "Пароль", value: password, setter: setPassword, type: "password" },
            ].map(({ label, value, setter, type }) => (
              <TextField
                key={label}
                label={label}
                type={type}
                fullWidth
                value={value}
                onChange={(e) => setter(e.target.value)}
                sx={{
                  "& .MuiOutlinedInput-root": {
                    color: "white",
                    "& fieldset": { borderColor: "rgba(255,255,255,0.2)" },
                    "&:hover fieldset": { borderColor: "#e94560" },
                  },
                  "& .MuiInputLabel-root": { color: "rgba(255,255,255,0.5)" },
                }}
              />
            ))}

            <Button
              variant="contained"
              fullWidth
              onClick={handleRegister}
              disabled={loading}
              sx={{
                mt: 1, py: 1.5,
                bgcolor: "#e94560",
                "&:hover": { bgcolor: "#c73652" },
                fontWeight: 700,
                borderRadius: 2,
              }}
            >
              Зарегистрироваться
            </Button>
          </Box>
        </Paper>
      </Container>
    </Box>
  );
}
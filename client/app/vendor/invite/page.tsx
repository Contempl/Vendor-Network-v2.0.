"use client";

import {
  Box, Typography, TextField, Button, Paper, Alert
} from "@mui/material";
import PersonAddIcon from "@mui/icons-material/PersonAdd";
import { useState } from "react";
import { inviteVendorUser } from "@/entities/vendor/vendor.api";
import ProtectedRoute from "@/entities/auth/ProtectedRoute";

function InvitePage() {
  const [email, setEmail] = useState("");
  const [inviteUrl, setInviteUrl] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const handleInvite = async () => {
    setLoading(true);
    setError(null);
    setInviteUrl("");
    try {
      const invitation = await inviteVendorUser({ email });
      setInviteUrl(`${window.location.origin}/register/${invitation.inviteId}`);
      setEmail("");
    } catch {
      setError("Не удалось отправить приглашение. Проверьте email и попробуйте снова.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", p: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" fontWeight={800} color="white" letterSpacing="-1px">
          Пригласить пользователя
        </Typography>
        <Typography color="rgba(255,255,255,0.3)" fontSize={14} mt={0.5}>
          Отправить приглашение на email
        </Typography>
      </Box>

      <Paper sx={{ bgcolor: "#161616", border: "1px solid rgba(255,255,255,0.07)", borderRadius: 3, p: 4, maxWidth: 500 }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 1.5, mb: 3 }}>
          <PersonAddIcon sx={{ color: "#e94560" }} />
          <Typography fontWeight={700} color="white">Новый пользователь</Typography>
        </Box>

        {inviteUrl && (
          <Alert severity="success" sx={{ mb: 3, bgcolor: "rgba(46,125,50,0.15)", color: "#81c784" }} onClose={() => setInviteUrl("")}>
            Share this registration link: <a href={inviteUrl}>{inviteUrl}</a>{" "}
            <Button size="small" onClick={() => void navigator.clipboard.writeText(inviteUrl)}>Copy</Button>
          </Alert>
        )}
        {error && (
          <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
            {error}
          </Alert>
        )}

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
          <Button
            variant="contained"
            onClick={handleInvite}
            disabled={loading || !email}
            sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2, fontWeight: 700, py: 1.5 }}
          >
            {loading ? "Отправка..." : "Отправить приглашение"}
          </Button>
        </Box>
      </Paper>
    </Box>
  );
}

export default function ProtectedInvitePage() {
  return <ProtectedRoute allowedRole="Admin" businessType="Vendor"><InvitePage /></ProtectedRoute>;
}

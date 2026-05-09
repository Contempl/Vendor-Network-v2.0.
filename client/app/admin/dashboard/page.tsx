"use client";

import {
  Box, Typography, Tabs, Tab, TextField, Button,
  Paper, Switch, FormControlLabel, Alert
} from "@mui/material";
import { useState } from "react";

import { BusinessInvitationData, DataForInviteDto } from "@/entities/auth/auth-types";
import { inviteBusiness, inviteOperatorUser, inviteVendorUser, removeOperator, removeVendor } from "@/entities/Admin/admin-api";


const inputSx = {
  "& .MuiOutlinedInput-root": {
    color: "white",
    "& fieldset": { borderColor: "rgba(255,255,255,0.2)" },
    "&:hover fieldset": { borderColor: "#e94560" },
  },
  "& .MuiInputLabel-root": { color: "rgba(255,255,255,0.5)" },
};

export default function AdminDashboard() {
  const [tab, setTab] = useState(0);
  const [success, setSuccess] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  // Invite Business
  const [businessForm, setBusinessForm] = useState<BusinessInvitationData>({
    businessIsVendor: true,
    businessName: "",
    businessAddress: "",
    businessEmail: "",
    firstName: "",
    lastName: "",
    userEmail: "",
  });

  // Invite User
  const [userInviteForm, setUserInviteForm] = useState<DataForInviteDto>({
    email: "",
    businessId: 0,
  });
  const [inviteIsVendor, setInviteIsVendor] = useState(true);

  // Remove
  const [removeId, setRemoveId] = useState("");
  const [removeIsVendor, setRemoveIsVendor] = useState(true);

  const handleInviteBusiness = async () => {
    setLoading(true); setError(null);
    try {
      await inviteBusiness(businessForm);
      setSuccess("Business is invited.");
    } catch {
      setError("Не удалось пригласить бизнес.");
    } finally { setLoading(false); }
  };

  const handleInviteUser = async () => {
    setLoading(true); setError(null);
    try {
      inviteIsVendor
        ? await inviteVendorUser(userInviteForm)
        : await inviteOperatorUser(userInviteForm);
      setSuccess("User was invited.");
    } catch {
      setError("Не удалось пригласить пользователя.");
    } finally { setLoading(false); }
  };

  const handleRemove = async () => {
    setLoading(true); setError(null);
    try {
      removeIsVendor
        ? await removeVendor(Number(removeId))
        : await removeOperator(Number(removeId));
      setSuccess("Deleted!");
    } catch {
      setError("Не удалось удалить бизнес.");
    } finally { setLoading(false); }
  };

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", p: 4 }}>
      <Typography variant="h4" fontWeight={800} color="white" letterSpacing="-1px" mb={1}>
        Панель администратора
      </Typography>
      <Typography color="rgba(255,255,255,0.3)" fontSize={14} mb={4}>
        Управление бизнесами и пользователями
      </Typography>

      {success && (
        <Alert severity="success" sx={{ mb: 3, bgcolor: "rgba(46,125,50,0.15)", color: "#81c784" }} onClose={() => setSuccess(null)}>
          {success}
        </Alert>
      )}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setError(null)}>
          {error}
        </Alert>
      )}

      <Tabs value={tab} onChange={(_e, v) => setTab(v)} sx={{
        mb: 4,
        "& .MuiTab-root": { color: "rgba(255,255,255,0.4)", fontWeight: 600 },
        "& .Mui-selected": { color: "#e94560 !important" },
        "& .MuiTabs-indicator": { bgcolor: "#e94560" },
      }}>
        <Tab label="Пригласить бизнес" />
        <Tab label="Пригласить пользователя" />
        <Tab label="Удалить" />
      </Tabs>

      {/* Tab 0 — Invite Business */}
      {tab === 0 && (
        <Paper sx={{ bgcolor: "#161616", border: "1px solid rgba(255,255,255,0.07)", borderRadius: 3, p: 4, maxWidth: 500 }}>
          <Typography fontWeight={700} color="white" mb={3}>Пригласить бизнес</Typography>
          <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <FormControlLabel
              control={<Switch checked={businessForm.businessIsVendor} onChange={(e) =>
                setBusinessForm({ ...businessForm, businessIsVendor: e.target.checked })} sx={{ "& .Mui-checked": { color: "#e94560" } }} />}
              label={<Typography color="white">{businessForm.businessIsVendor ? "Vendor" : "Operator"}</Typography>}
            />
            {(["businessName", "businessAddress", "businessEmail", "firstName", "lastName", "userEmail"] as const).map((field) => (
              <TextField key={field} label={field} value={businessForm[field]}
                onChange={(e) => setBusinessForm({ ...businessForm, [field]: e.target.value })}
                sx={inputSx} />
            ))}
            <Button variant="contained" onClick={handleInviteBusiness} disabled={loading}
              sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2, fontWeight: 700, py: 1.5 }}>
              {loading ? "Отправка..." : "Отправить приглашение"}
            </Button>
          </Box>
        </Paper>
      )}

      {/* Tab 1 — Invite User */}
      {tab === 1 && (
        <Paper sx={{ bgcolor: "#161616", border: "1px solid rgba(255,255,255,0.07)", borderRadius: 3, p: 4, maxWidth: 500 }}>
          <Typography fontWeight={700} color="white" mb={3}>Пригласить пользователя</Typography>
          <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <FormControlLabel
              control={<Switch checked={inviteIsVendor} onChange={(e) => setInviteIsVendor(e.target.checked)}
                sx={{ "& .Mui-checked": { color: "#e94560" } }} />}
              label={<Typography color="white">{inviteIsVendor ? "Vendor User" : "Operator User"}</Typography>}
            />
            <TextField label="Email" value={userInviteForm.email}
              onChange={(e) => setUserInviteForm({ ...userInviteForm, email: e.target.value })} sx={inputSx} />
            <TextField label="Business ID" type="number" value={userInviteForm.businessId}
              onChange={(e) => setUserInviteForm({ ...userInviteForm, businessId: Number(e.target.value) })} sx={inputSx} />
            <Button variant="contained" onClick={handleInviteUser} disabled={loading}
              sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2, fontWeight: 700, py: 1.5 }}>
              {loading ? "Отправка..." : "Пригласить"}
            </Button>
          </Box>
        </Paper>
      )}

      {/* Tab 2 — Remove */}
      {tab === 2 && (
        <Paper sx={{ bgcolor: "#161616", border: "1px solid rgba(255,255,255,0.07)", borderRadius: 3, p: 4, maxWidth: 500 }}>
          <Typography fontWeight={700} color="white" mb={3}>Удалить бизнес</Typography>
          <Box sx={{ display: "flex", flexDirection: "column", gap: 2 }}>
            <FormControlLabel
              control={<Switch checked={removeIsVendor} onChange={(e) => setRemoveIsVendor(e.target.checked)}
                sx={{ "& .Mui-checked": { color: "#e94560" } }} />}
              label={<Typography color="white">{removeIsVendor ? "Vendor" : "Operator"}</Typography>}
            />
            <TextField label="ID бизнеса" type="number" value={removeId}
              onChange={(e) => setRemoveId(e.target.value)} sx={inputSx} />
            <Button variant="contained" onClick={handleRemove} disabled={loading}
              sx={{ bgcolor: "#c62828", "&:hover": { bgcolor: "#b71c1c" }, borderRadius: 2, fontWeight: 700, py: 1.5 }}>
              {loading ? "Удаление..." : "Удалить"}
            </Button>
          </Box>
        </Paper>
      )}
    </Box>
  );
}
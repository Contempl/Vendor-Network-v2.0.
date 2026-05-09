"use client";

import {
  Avatar, Box, Button, Chip, CircularProgress,
  Divider, Paper, TextField, Typography
} from "@mui/material";
import BusinessIcon from "@mui/icons-material/Business";
import EditIcon from "@mui/icons-material/Edit";
import SaveIcon from "@mui/icons-material/Save";
import LocationOnIcon from "@mui/icons-material/LocationOn";
import WorkIcon from "@mui/icons-material/Work";
import { useEffect, useState } from "react";
import { useOperatorStore } from "@/entities/operator/operator-store";
import { getOperator, updateOperator } from "@/entities/operator/operators.api";
import { UpdateOperatorDto } from "@/entities/operator/operator.types";

export default function OperatorPage() {
  const { operator, setOperator } = useOperatorStore();
  const [isEditing, setIsEditing] = useState(false);
  const [form, setForm] = useState<UpdateOperatorDto | null>(null);

  useEffect(() => {
    getOperator().then(setOperator);
  }, []);

  const handleEdit = () => {
    setIsEditing(true);
    setForm({
      businessName: operator?.businessName ?? "",
      address: operator?.address ?? "",
      email: null,
      logoUrl: null,
      occupation: operator?.occupation ?? "",
    });
  };

  const handleSave = async () => {
    if (!form) return;
    const updated = await updateOperator(form);
    setOperator(updated);
    setIsEditing(false);
  };

  if (!operator) return (
    <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "100vh", bgcolor: "#0d0d0d" }}>
      <CircularProgress sx={{ color: "#e94560" }} />
    </Box>
  );

  const fieldSx = {
    "& .MuiOutlinedInput-root": {
      color: "white",
      "& fieldset": { borderColor: "rgba(255,255,255,0.2)" },
      "&:hover fieldset": { borderColor: "#e94560" },
    },
    "& .MuiInputLabel-root": { color: "rgba(255,255,255,0.5)" },
  };

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", p: 4 }}>
      <Box sx={{ mb: 4, display: "flex", alignItems: "center", justifyContent: "space-between" }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          <Avatar sx={{ bgcolor: "#e94560", width: 56, height: 56 }}>
            <BusinessIcon fontSize="large" />
          </Avatar>
          <Box>
            <Typography variant="h4" fontWeight={800} color="white" letterSpacing="-1px">
              {operator.businessName}
            </Typography>
            <Chip label="Operator" size="small" sx={{ bgcolor: "rgba(233,69,96,0.15)", color: "#e94560", fontWeight: 700, mt: 0.5 }} />
          </Box>
        </Box>
        {isEditing ? (
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={handleSave}
            sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2, fontWeight: 700 }}
          >
            Сохранить
          </Button>
        ) : (
          <Button
            variant="outlined"
            startIcon={<EditIcon />}
            onClick={handleEdit}
            sx={{ borderColor: "#e94560", color: "#e94560", "&:hover": { bgcolor: "rgba(233,69,96,0.1)", borderColor: "#e94560" } }}
          >
            Редактировать
          </Button>
        )}
      </Box>

      <Divider sx={{ borderColor: "rgba(255,255,255,0.08)", mb: 4 }} />

      <Paper sx={{ bgcolor: "#161616", border: "1px solid rgba(255,255,255,0.07)", borderRadius: 3, p: 4, maxWidth: 600 }}>
        <Typography variant="overline" color="rgba(255,255,255,0.3)" fontWeight={700} letterSpacing={2}>
          Информация о бизнесе
        </Typography>

        <Box sx={{ mt: 3, display: "flex", flexDirection: "column", gap: 3 }}>
          <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
            <BusinessIcon sx={{ color: "#e94560" }} />
            {isEditing ? (
              <TextField
                label="Business Name"
                value={form?.businessName ?? ""}
                onChange={(e) => setForm({ ...form!, businessName: e.target.value })}
                sx={fieldSx}
              />
            ) : (
              <Box>
                <Typography variant="caption" color="rgba(255,255,255,0.3)">Business Name</Typography>
                <Typography color="white" fontWeight={600}>{operator.businessName}</Typography>
              </Box>
            )}
          </Box>

          <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
            <LocationOnIcon sx={{ color: "#e94560" }} />
            {isEditing ? (
              <TextField
                label="Address"
                value={form?.address ?? ""}
                onChange={(e) => setForm({ ...form!, address: e.target.value })}
                sx={fieldSx}
              />
            ) : (
              <Box>
                <Typography variant="caption" color="rgba(255,255,255,0.3)">Address</Typography>
                <Typography color="white" fontWeight={600}>{operator.address}</Typography>
              </Box>
            )}
          </Box>

          <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
            <WorkIcon sx={{ color: "#e94560" }} />
            {isEditing ? (
              <TextField
                label="Occupation"
                value={form?.occupation ?? ""}
                onChange={(e) => setForm({ ...form!, occupation: e.target.value })}
                sx={fieldSx}
              />
            ) : (
              <Box>
                <Typography variant="caption" color="rgba(255,255,255,0.3)">Occupation</Typography>
                <Typography color="white" fontWeight={600}>{operator.occupation}</Typography>
              </Box>
            )}
          </Box>
        </Box>
      </Paper>
    </Box>
  );
}

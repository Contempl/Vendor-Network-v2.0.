"use client";

import { getBusinessIdFromToken } from "@/entities/auth/auth-utils";
import { useVendorStore } from "@/entities/vendor/vendor-store";
import { getVendor, updateVendor } from "@/entities/vendor/vendor.api";
import { Avatar, 
  Box, 
  Button,
  Chip,
  CircularProgress,
  Divider, Paper, TextField, Typography } from "@mui/material";
  import BusinessIcon from "@mui/icons-material/Business";
import EditIcon from "@mui/icons-material/Edit";
import SaveIcon from "@mui/icons-material/Save";
import LocationOnIcon from "@mui/icons-material/LocationOn";
import EmailIcon from "@mui/icons-material/Email";
import { useEffect, useState } from "react";
import { UpdateVendorDto } from "@/entities/vendor/vendor-types";

export default function VendorPage() {

  const { vendor, setVendor } = useVendorStore();

  const [isEditing, setIsEditing] = useState(false);
  const [form, setForm] = useState<UpdateVendorDto | null>(null)

  useEffect(() => {
  const fetchVendor = async () => {
      const businessId = getBusinessIdFromToken();
      if (businessId) {
          const variable = await getVendor(Number(businessId));
          setVendor(variable);
      }
  };
  fetchVendor();
  }, []);


  const handleEdit = () => {
    setIsEditing(true);
    setForm({
      businessName: vendor?.businessName || "",
      address: vendor?.address || "",
      email: null,
    });
  }

  const handleSave = async () => {
    if (!form) return;
    const updated = await updateVendor(form);
    setVendor(updated);
    setIsEditing(false);
  }

  if (!vendor) return (
    <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "100vh", bgcolor: "#0d0d0d" }}>
      <CircularProgress sx={{ color: "#e94560" }} />
    </Box>
  );

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", p: 4 }}>
      {/* Header */}
      <Box sx={{ mb: 4, display: "flex", alignItems: "center", justifyContent: "space-between" }}>
        <Box sx={{ display: "flex", alignItems: "center", gap: 2 }}>
          <Avatar sx={{ bgcolor: "#e94560", width: 56, height: 56 }}>
            <BusinessIcon fontSize="large" />
          </Avatar>
          <Box>
            <Typography variant="h4" fontWeight={800} color="white" letterSpacing="-1px">
              {vendor.businessName}
            </Typography>
            <Chip label="Vendor" size="small" sx={{ bgcolor: "rgba(233,69,96,0.15)", color: "#e94560", fontWeight: 700, mt: 0.5 }} />
          </Box>
          {isEditing 
          ? <Button onClick={handleSave}>Save</Button> 
          : <Button onClick={handleEdit}>Edit</Button>
}
        </Box>
        <Button
          variant="outlined"
          startIcon={<EditIcon />}
          onClick={handleEdit}
          sx={{ borderColor: "#e94560", color: "#e94560", "&:hover": { bgcolor: "rgba(233,69,96,0.1)", borderColor: "#e94560" } }}
        >
          Редактировать
        </Button>
      </Box>
      <Divider sx={{ borderColor: "rgba(255,255,255,0.08)", mb: 4 }} />

      {/* Info Card */}
      <Paper sx={{ bgcolor: "#161616", border: "1px solid rgba(255,255,255,0.07)", borderRadius: 3, p: 4, maxWidth: 600 }}>
        <Typography variant="overline" color="rgba(255,255,255,0.3)" fontWeight={700} letterSpacing={2}>
          Информация о бизнесе
        </Typography>

        <Box sx={{ mt: 3, display: "flex", flexDirection: "column", gap: 3 }}>
          {/* Business Name */}
          <Box sx={{ display: "flex", alignItems: "flex-start", gap: 2 }}>
            <BusinessIcon sx={{ color: "#e94560", mt: 0.5 }} />
              <Typography variant="caption" color="rgba(255,255,255,0.3)">Business Name</Typography>
              {isEditing 
                ? <TextField value={form?.businessName || ""} onChange={(e) => setForm({...form!, businessName: e.target.value})} sx={{
                    "& .MuiOutlinedInput-root": {
                      color: "white",
                      "& fieldset": { borderColor: "rgba(255,255,255,0.2)" },
                      "&:hover fieldset": { borderColor: "#e94560" },
                    },
                    "& .MuiInputLabel-root": { color: "rgba(255,255,255,0.5)" },
                  }} />
                : <Typography color="white" fontWeight={600}>{vendor.businessName}</Typography>
              }
          </Box>

          {/* Address */}
          <Box sx={{ display: "flex", alignItems: "flex-start", gap: 2 }}>
            <LocationOnIcon sx={{ color: "#e94560", mt: 0.5 }} />
            <Typography variant="caption" color="rgba(255,255,255,0.3)">Address</Typography>
            {isEditing 
              ? <TextField value={form?.address || ""} onChange={(e) => setForm({...form!, address: e.target.value})} sx={{
                    "& .MuiOutlinedInput-root": {
                      color: "white",
                      "& fieldset": { borderColor: "rgba(255,255,255,0.2)" },
                      "&:hover fieldset": { borderColor: "#e94560" },
                    },
                    "& .MuiInputLabel-root": { color: "rgba(255,255,255,0.5)" },
                  }} />
              : <Typography color="white" fontWeight={600}>{vendor.address}</Typography>
            }
          </Box>

          
        </Box>
      </Paper>
    </Box>
  );
}
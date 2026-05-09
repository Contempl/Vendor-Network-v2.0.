"use client";

import React, { useEffect, useState } from "react";
import {
  Box, Typography, Chip, CircularProgress, Paper,
  Button, Divider, IconButton, Dialog, DialogTitle,
  DialogContent, DialogActions, TextField
} from "@mui/material";
import LocationOnIcon from "@mui/icons-material/LocationOn";
import RadioButtonCheckedIcon from "@mui/icons-material/RadioButtonChecked";
import MyLocationIcon from "@mui/icons-material/MyLocation";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import { useRouter } from "next/navigation";
import { getFacility, getFacilityServices, updateFacility, deleteFacility } from "@/entities/vendor/vendor.api";
import { VendorFacility, VendorFacilityService, UpdateVendorFacilityDto } from "@/entities/vendor/vendor-facility-types";
import { getBusinessIdFromToken } from "@/entities/auth/auth-utils";

const fieldSx = {
  "& .MuiOutlinedInput-root": {
    color: "white",
    "& fieldset": { borderColor: "rgba(255,255,255,0.2)" },
    "&:hover fieldset": { borderColor: "#e94560" },
  },
  "& .MuiInputLabel-root": { color: "rgba(255,255,255,0.5)" },
};

export default function FacilityDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = React.use(params);
  const router = useRouter();

  const [facility, setFacility] = useState<VendorFacility | null>(null);
  const [services, setServices] = useState<VendorFacilityService[]>([]);
  const [editOpen, setEditOpen] = useState(false);
  const [editForm, setEditForm] = useState<UpdateVendorFacilityDto | null>(null);
  const [deleteOpen, setDeleteOpen] = useState(false);

  useEffect(() => {
    const load = async () => {
      const [f, s] = await Promise.all([
        getFacility(Number(id)),
        getFacilityServices(Number(id)),
      ]);
      setFacility(f);
      setServices(s);
    };
    load();
  }, [id]);

  const handleEditOpen = () => {
    if (!facility) return;
    setEditForm({
      name: facility.name,
      location: facility.location,
      latitude: facility.latitude,
      longitude: facility.longitude,
      radiusOfWork: facility.radiusOfWork,
      services: null,
    });
    setEditOpen(true);
  };

  const handleEditSave = async () => {
    if (!editForm) return;
    const updated = await updateFacility(Number(id), editForm);
    setFacility({ ...facility!, ...updated });
    setEditOpen(false);
  };

  const handleDelete = async () => {
    const vendorId = getBusinessIdFromToken();
    await deleteFacility(Number(vendorId), Number(id));
    router.push("/vendor/facilities");
  };

  if (!facility) return (
    <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "100vh", bgcolor: "#0d0d0d" }}>
      <CircularProgress sx={{ color: "#e94560" }} />
    </Box>
  );

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", p: 4 }}>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 3 }}>
        <Button
          startIcon={<ArrowBackIcon />}
          onClick={() => router.push("/vendor/facilities")}
          sx={{ color: "rgba(255,255,255,0.5)", "&:hover": { color: "white" } }}
        >
          Назад
        </Button>
        <Box sx={{ display: "flex", gap: 1 }}>
          <IconButton onClick={handleEditOpen} sx={{ color: "rgba(255,255,255,0.4)", "&:hover": { color: "#e94560" } }}>
            <EditIcon />
          </IconButton>
          <IconButton onClick={() => setDeleteOpen(true)} sx={{ color: "rgba(255,255,255,0.4)", "&:hover": { color: "#e94560" } }}>
            <DeleteIcon />
          </IconButton>
        </Box>
      </Box>

      <Typography variant="h4" fontWeight={800} color="white" letterSpacing="-1px" mb={1}>
        {facility.name ?? "Без названия"}
      </Typography>
      <Divider sx={{ borderColor: "rgba(255,255,255,0.08)", mb: 4 }} />

      <Paper sx={{ bgcolor: "#161616", border: "1px solid rgba(255,255,255,0.07)", borderRadius: 3, p: 4, maxWidth: 600, mb: 3 }}>
        <Typography variant="overline" color="rgba(255,255,255,0.3)" fontWeight={700} letterSpacing={2}>
          Информация
        </Typography>

        <Box sx={{ mt: 3, display: "flex", flexDirection: "column", gap: 2.5 }}>
          <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
            <LocationOnIcon sx={{ color: "#e94560" }} />
            <Box>
              <Typography variant="caption" color="rgba(255,255,255,0.3)">Адрес</Typography>
              <Typography color="white" fontWeight={600}>{facility.location}</Typography>
            </Box>
          </Box>

          <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
            <RadioButtonCheckedIcon sx={{ color: "#e94560" }} />
            <Box>
              <Typography variant="caption" color="rgba(255,255,255,0.3)">Радиус работы</Typography>
              <Typography color="white" fontWeight={600}>{facility.radiusOfWork} км</Typography>
            </Box>
          </Box>

          <Box sx={{ display: "flex", alignItems: "center", gap: 1.5 }}>
            <MyLocationIcon sx={{ color: "#e94560" }} />
            <Box>
              <Typography variant="caption" color="rgba(255,255,255,0.3)">Координаты</Typography>
              <Typography color="white" fontWeight={600}>
                {facility.latitude}, {facility.longitude}
              </Typography>
            </Box>
          </Box>
        </Box>
      </Paper>

      <Paper sx={{ bgcolor: "#161616", border: "1px solid rgba(255,255,255,0.07)", borderRadius: 3, p: 4, maxWidth: 600 }}>
        <Typography variant="overline" color="rgba(255,255,255,0.3)" fontWeight={700} letterSpacing={2}>
          Услуги
        </Typography>
        <Box sx={{ mt: 2, display: "flex", flexWrap: "wrap", gap: 1 }}>
          {services.length === 0 ? (
            <Typography color="rgba(255,255,255,0.2)">Нет услуг</Typography>
          ) : (
            services.map((s) => (
              <Chip
                key={s.id}
                label={s.name}
                sx={{ bgcolor: "rgba(233,69,96,0.12)", color: "#e94560", fontWeight: 600 }}
              />
            ))
          )}
        </Box>
      </Paper>

      {/* Edit Dialog */}
      <Dialog open={editOpen} onClose={() => setEditOpen(false)}
        PaperProps={{ sx: { bgcolor: "#161616", color: "white", borderRadius: 3, minWidth: 400 } }}>
        <DialogTitle fontWeight={700}>Редактировать Facility</DialogTitle>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 2 }}>
          <TextField label="Название" value={editForm?.name ?? ""} onChange={(e) => setEditForm({ ...editForm!, name: e.target.value })} sx={fieldSx} />
          <TextField label="Адрес" value={editForm?.location ?? ""} onChange={(e) => setEditForm({ ...editForm!, location: e.target.value })} sx={fieldSx} />
          <TextField label="Радиус работы" type="number" value={editForm?.radiusOfWork ?? ""} onChange={(e) => setEditForm({ ...editForm!, radiusOfWork: Number(e.target.value) })} sx={fieldSx} />
          <TextField label="Latitude" type="number" value={editForm?.latitude ?? ""} onChange={(e) => setEditForm({ ...editForm!, latitude: Number(e.target.value) })} sx={fieldSx} />
          <TextField label="Longitude" type="number" value={editForm?.longitude ?? ""} onChange={(e) => setEditForm({ ...editForm!, longitude: Number(e.target.value) })} sx={fieldSx} />
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={() => setEditOpen(false)} sx={{ color: "rgba(255,255,255,0.5)" }}>Отмена</Button>
          <Button variant="contained" onClick={handleEditSave} sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2 }}>
            Сохранить
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Confirm Dialog */}
      <Dialog open={deleteOpen} onClose={() => setDeleteOpen(false)}
        PaperProps={{ sx: { bgcolor: "#161616", color: "white", borderRadius: 3 } }}>
        <DialogTitle fontWeight={700}>Удалить facility?</DialogTitle>
        <DialogContent>
          <Typography color="rgba(255,255,255,0.5)">Это действие необратимо.</Typography>
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={() => setDeleteOpen(false)} sx={{ color: "rgba(255,255,255,0.5)" }}>Отмена</Button>
          <Button variant="contained" onClick={handleDelete} sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2 }}>
            Удалить
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

"use client";

import {
  Box, Typography, Button, Card, CardContent,
  CardActions, Chip, IconButton,
  Dialog, DialogTitle, DialogContent, DialogActions,
  TextField
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import LocationOnIcon from "@mui/icons-material/LocationOn";
import RadioButtonCheckedIcon from "@mui/icons-material/RadioButtonChecked";
import { useVendorStore } from "@/entities/vendor/vendor-store";
import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { getBusinessIdFromToken } from "@/entities/auth/auth-utils";
import { createFacility, deleteFacility, getVendorFacilities, updateFacility } from "@/entities/vendor/vendor.api";
import { VendorFacilityDto, VendorGetFacilitiesDto } from "@/entities/vendor/vendor-facility-types";

const emptyForm: VendorFacilityDto = {
  name: "",
  location: "",
  longitude: 0,
  latitude: 0,
  radiusOfWork: 0,
  services: [],
};

export default function FacilitiesPage() {
  const { facilities, setFacilities } = useVendorStore();
  const router = useRouter();
  const [openDialog, setOpenDialog] = useState(false);
  const [form, setForm] = useState<VendorFacilityDto>(emptyForm);
  const [editingFacility, setEditingFacility] = useState<VendorGetFacilitiesDto  | null>(null);
  const [editForm, setEditForm] = useState<VendorFacilityDto | null>(null);

    useEffect(() => {
    const businessId = getBusinessIdFromToken();
    if (businessId) {
      getVendorFacilities().then((facilities) => {
        setFacilities(facilities);
      });
    }
  }, [setFacilities]);

  const handleCreate = async () => {
    const newFacility = await createFacility(form);
    setFacilities([...facilities ?? [], newFacility]);
    setOpenDialog(false);
    setForm(emptyForm);
  }

  const handleEditOpen = (facility: VendorGetFacilitiesDto) => {
    setEditingFacility(facility);
    setEditForm({
      name: facility.name,
      location: facility.location,
      latitude: facility.latitude,
      longitude: facility.longitude,
      radiusOfWork: facility.radiusOfWork,
      services: facility.services
    }) 
  };

  const handleEditSave = async () => {
    if (editingFacility === null || editForm === null)
    {
        return;
    }
    const updated = await updateFacility(editingFacility.id, editForm);
    setFacilities(facilities.map(f => f.id === editingFacility.id ? updated : f));
    setEditingFacility(null);
  }

  const handleDelete = async (vendorId: number, facilityId: number) => {
    await deleteFacility(vendorId, facilityId);
    setFacilities(facilities?.filter((f) => f.id !== facilityId) ?? []);
  }

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", p: 4 }}>
      {/* Header */}
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 4 }}>
        <Box>
          <Typography variant="h4" fontWeight={800} color="white" letterSpacing="-1px">
            Facilities
          </Typography>
          <Typography color="rgba(255,255,255,0.3)" fontSize={14} mt={0.5}>
            Управление локациями и услугами
          </Typography>
        </Box>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => setOpenDialog(true)}
          sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2, fontWeight: 700 }}
        >
          Добавить
        </Button>
      </Box>

      {/* Facilities Grid */}
      {facilities && facilities.length === 0 ? (
        <Box sx={{ textAlign: "center", mt: 10 }}>
          <Typography color="rgba(255,255,255,0.2)" fontSize={18}>
            Нет добавленных facilities
          </Typography>
        </Box>
      ) : (
        <Box sx={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: 2 }}>
          {facilities?.map((facility) => (
            <Card key={facility.id} onClick={() => router.push(`/vendor/facilities/${facility.id}`)} sx={{
              bgcolor: "#161616",
              border: "1px solid rgba(255,255,255,0.07)",
              borderRadius: 3,
              color: "white",
              cursor: "pointer",
              "&:hover": { border: "1px solid rgba(233,69,96,0.4)" },
            }}>
              <CardContent>
                <Typography fontWeight={700} fontSize={18} mb={1}>
                  {facility.name ?? "Без названия"}
                </Typography>
                <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
                  <LocationOnIcon sx={{ color: "#e94560", fontSize: 16 }} />
                  <Typography fontSize={13} color="rgba(255,255,255,0.5)">
                    {facility.location}
                  </Typography>
                </Box>
                <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 2 }}>
                  <RadioButtonCheckedIcon sx={{ color: "#e94560", fontSize: 16 }} />
                  <Typography fontSize={13} color="rgba(255,255,255,0.5)">
                    Радиус: {facility.radiusOfWork} км
                  </Typography>
                </Box>
                <Box sx={{ display: "flex", flexWrap: "wrap", gap: 0.5 }}>
                  {facility.services?.map((s, index) => (
                      <Chip key={index} label={s} size="small"
                        sx={{ bgcolor: "rgba(233,69,96,0.12)", color: "#e94560", fontSize: 11 }} />
                  ))}
                </Box>
              </CardContent>
              <CardActions sx={{ justifyContent: "flex-end", px: 2, pb: 2 }}>
                <IconButton size="small" sx={{ color: "rgba(255,255,255,0.3)" }} onClick={(e) => { e.stopPropagation(); handleEditOpen(facility); }}>
                  <EditIcon fontSize="small" />
                </IconButton>
                <IconButton
                  size="small"
                  sx={{ color: "rgba(255,255,255,0.3)", "&:hover": { color: "#e94560" } }}
                  onClick={async (e) => { e.stopPropagation(); await handleDelete(Number(getBusinessIdFromToken()), facility.id); }}
                >
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </CardActions>
            </Card>
          ))}
        </Box>
      )}

      <Dialog open={editingFacility !== null} onClose={() => setEditingFacility(null)}
        PaperProps={{ sx: { bgcolor: "#161616", color: "white", borderRadius: 3, minWidth: 400 } }}>
        <DialogTitle fontWeight={700}>Редактировать Facility</DialogTitle>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 2 }}>
          {(["name", "location", "radiusOfWork"] as const).map((field) => (
            <TextField
              key={field}
              label={field}
              value={editForm?.[field] ?? ""}
              onChange={(e) => setEditForm({ ...editForm!, [field]: e.target.value })}
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
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={() => setEditingFacility(null)} sx={{ color: "rgba(255,255,255,0.5)" }}>
            Отмена
          </Button>
          <Button
            variant="contained"
            onClick={async () => await handleEditSave()}
            sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2 }}
          >
            Сохранить
          </Button>
        </DialogActions>
      </Dialog>

      {/* Add Dialog */}
      <Dialog open={openDialog} onClose={() => setOpenDialog(false)}
        PaperProps={{ sx: { bgcolor: "#161616", color: "white", borderRadius: 3, minWidth: 400 } }}>
        <DialogTitle fontWeight={700}>Новая Facility</DialogTitle>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 2 }}>
          {(["name", "location", "radiusOfWork"] as const).map((field) => (
            <TextField
              key={field}
              label={field}
              value={form[field]}
              onChange={(e) => setForm({ ...form, [field]: e.target.value })}
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
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={() => setOpenDialog(false)} sx={{ color: "rgba(255,255,255,0.5)" }}>
            Отмена
          </Button>
          <Button
            variant="contained"
            onClick={async () => await handleCreate()}
            sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2 }}
          >
            Создать
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}
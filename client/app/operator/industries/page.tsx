"use client";

import {
  Box, Typography, Button, Card, CardContent,
  CardActions, IconButton,
  Dialog, DialogTitle, DialogContent, DialogActions,
  TextField
} from "@mui/material";
import AddIcon from "@mui/icons-material/Add";
import DeleteIcon from "@mui/icons-material/Delete";
import LocationOnIcon from "@mui/icons-material/LocationOn";
import FactoryIcon from "@mui/icons-material/Factory";
import { useEffect, useState } from "react";
import { useOperatorStore } from "@/entities/operator/operator-store";
import { getOperatorIndustries, createOperatorIndustry, deleteOperatorIndustry } from "@/entities/operator/operators.api";
import { OperatorIndustryCreationDto } from "@/entities/operator/operator.types";

const emptyForm: OperatorIndustryCreationDto = {
  name: "",
  address: "",
  latitude: 0,
  longitude: 0,
};

const fieldSx = {
  "& .MuiOutlinedInput-root": {
    color: "white",
    "& fieldset": { borderColor: "rgba(255,255,255,0.2)" },
    "&:hover fieldset": { borderColor: "#e94560" },
  },
  "& .MuiInputLabel-root": { color: "rgba(255,255,255,0.5)" },
};

export default function IndustriesPage() {
  const { industries, setIndustries } = useOperatorStore();
  const [openDialog, setOpenDialog] = useState(false);
  const [form, setForm] = useState<OperatorIndustryCreationDto>(emptyForm);

  useEffect(() => {
    getOperatorIndustries().then(setIndustries);
  }, []);

  const handleCreate = async () => {
    const created = await createOperatorIndustry(form);
    setIndustries([...industries, created]);
    setOpenDialog(false);
    setForm(emptyForm);
  };

  const handleDelete = async (id: number) => {
    await deleteOperatorIndustry(id);
    setIndustries(industries.filter((i) => i.id !== id));
  };

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", p: 4 }}>
      <Box sx={{ display: "flex", justifyContent: "space-between", alignItems: "center", mb: 4 }}>
        <Box>
          <Typography variant="h4" fontWeight={800} color="white" letterSpacing="-1px">
            Industries
          </Typography>
          <Typography color="rgba(255,255,255,0.3)" fontSize={14} mt={0.5}>
            Управление промышленными объектами
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

      {industries.length === 0 ? (
        <Box sx={{ textAlign: "center", mt: 10 }}>
          <Typography color="rgba(255,255,255,0.2)" fontSize={18}>
            Нет добавленных industries
          </Typography>
        </Box>
      ) : (
        <Box sx={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))", gap: 2 }}>
          {industries.map((industry) => (
            <Card key={industry.id} sx={{
              bgcolor: "#161616",
              border: "1px solid rgba(255,255,255,0.07)",
              borderRadius: 3,
              color: "white",
            }}>
              <CardContent>
                <Box sx={{ display: "flex", alignItems: "center", gap: 1, mb: 1 }}>
                  <FactoryIcon sx={{ color: "#e94560", fontSize: 20 }} />
                  <Typography fontWeight={700} fontSize={18}>{industry.name}</Typography>
                </Box>
                <Box sx={{ display: "flex", alignItems: "center", gap: 1 }}>
                  <LocationOnIcon sx={{ color: "#e94560", fontSize: 16 }} />
                  <Typography fontSize={13} color="rgba(255,255,255,0.5)">{industry.address}</Typography>
                </Box>
              </CardContent>
              <CardActions sx={{ justifyContent: "flex-end", px: 2, pb: 2 }}>
                <IconButton
                  size="small"
                  sx={{ color: "rgba(255,255,255,0.3)", "&:hover": { color: "#e94560" } }}
                  onClick={() => handleDelete(industry.id)}
                >
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </CardActions>
            </Card>
          ))}
        </Box>
      )}

      <Dialog open={openDialog} onClose={() => setOpenDialog(false)}
        PaperProps={{ sx: { bgcolor: "#161616", color: "white", borderRadius: 3, minWidth: 400 } }}>
        <DialogTitle fontWeight={700}>Новый Industry</DialogTitle>
        <DialogContent sx={{ display: "flex", flexDirection: "column", gap: 2, pt: 2 }}>
          <TextField
            label="Название"
            value={form.name}
            onChange={(e) => setForm({ ...form, name: e.target.value })}
            sx={fieldSx}
          />
          <TextField
            label="Адрес"
            value={form.address}
            onChange={(e) => setForm({ ...form, address: e.target.value })}
            sx={fieldSx}
          />
          <TextField
            label="Latitude"
            type="number"
            value={form.latitude}
            onChange={(e) => setForm({ ...form, latitude: Number(e.target.value) })}
            sx={fieldSx}
          />
          <TextField
            label="Longitude"
            type="number"
            value={form.longitude}
            onChange={(e) => setForm({ ...form, longitude: Number(e.target.value) })}
            sx={fieldSx}
          />
        </DialogContent>
        <DialogActions sx={{ p: 3 }}>
          <Button onClick={() => setOpenDialog(false)} sx={{ color: "rgba(255,255,255,0.5)" }}>
            Отмена
          </Button>
          <Button
            variant="contained"
            onClick={handleCreate}
            sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2 }}
          >
            Создать
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
}

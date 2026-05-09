"use client";

import React, { useEffect, useState } from "react";
import {
  Box, Typography, Chip, CircularProgress, Paper,
  Button, Divider
} from "@mui/material";
import LocationOnIcon from "@mui/icons-material/LocationOn";
import RadioButtonCheckedIcon from "@mui/icons-material/RadioButtonChecked";
import MyLocationIcon from "@mui/icons-material/MyLocation";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { useRouter } from "next/navigation";
import { getFacility, getFacilityServices } from "@/entities/vendor/vendor.api";
import { VendorFacility, VendorFacilityService } from "@/entities/vendor/vendor-facility-types";

export default function FacilityDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = React.use(params);
  const router = useRouter();

  const [facility, setFacility] = useState<VendorFacility | null>(null);
  const [services, setServices] = useState<VendorFacilityService[]>([]);

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

  if (!facility) return (
    <Box sx={{ display: "flex", justifyContent: "center", alignItems: "center", minHeight: "100vh", bgcolor: "#0d0d0d" }}>
      <CircularProgress sx={{ color: "#e94560" }} />
    </Box>
  );

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", p: 4 }}>
      <Button
        startIcon={<ArrowBackIcon />}
        onClick={() => router.push("/vendor/facilities")}
        sx={{ color: "rgba(255,255,255,0.5)", mb: 3, "&:hover": { color: "white" } }}
      >
        Назад
      </Button>

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
    </Box>
  );
}

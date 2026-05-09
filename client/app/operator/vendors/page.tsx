"use client";

import {
  Box, Typography, TextField, Button, Paper, InputAdornment
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import BusinessIcon from "@mui/icons-material/Business";
import LocationOnIcon from "@mui/icons-material/LocationOn";
import { useState } from "react";
import { getVendors } from "@/entities/operator/operators.api";
import { BusinessFrontEndDto } from "@/entities/vendor/vendor-types";
import { SortOrder } from "@/entities/operator/operator.types";

export default function VendorsPage() {
  const [search, setSearch] = useState("");
  const [vendors, setVendors] = useState<BusinessFrontEndDto[]>([]);

  const handleSearch = async () => {
    const result = await getVendors({
      vendorName: search,
      sortOrder: SortOrder.Ascending,
      pageSize: 20,
      pageNumber: 1,
    });
    setVendors(result);
  };

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", p: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" fontWeight={800} color="white" letterSpacing="-1px">
          Поиск вендоров
        </Typography>
        <Typography color="rgba(255,255,255,0.3)" fontSize={14} mt={0.5}>
          Найдите вендоров для сотрудничества
        </Typography>
      </Box>

      <Box sx={{ display: "flex", gap: 2, mb: 4, maxWidth: 600 }}>
        <TextField
          fullWidth
          placeholder="Название вендора..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          onKeyDown={(e) => e.key === "Enter" && handleSearch()}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon sx={{ color: "rgba(255,255,255,0.3)" }} />
              </InputAdornment>
            ),
          }}
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
          onClick={handleSearch}
          sx={{ bgcolor: "#e94560", "&:hover": { bgcolor: "#c73652" }, borderRadius: 2, fontWeight: 700, px: 3 }}
        >
          Найти
        </Button>
      </Box>

      {vendors.length === 0 ? (
        <Typography color="rgba(255,255,255,0.2)" fontSize={16} mt={6} textAlign="center">
          Введите название вендора для поиска
        </Typography>
      ) : (
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2, maxWidth: 600 }}>
          {vendors.map((vendor) => (
            <Paper key={vendor.id} sx={{
              bgcolor: "#161616",
              border: "1px solid rgba(255,255,255,0.07)",
              borderRadius: 3,
              p: 3,
              display: "flex",
              alignItems: "center",
              gap: 2,
              "&:hover": { borderColor: "rgba(233,69,96,0.3)", cursor: "pointer" }
            }}>
              <Box sx={{
                bgcolor: "rgba(233,69,96,0.12)",
                borderRadius: 2,
                p: 1.5,
                display: "flex",
                alignItems: "center"
              }}>
                <BusinessIcon sx={{ color: "#e94560" }} />
              </Box>
              <Box>
                <Typography fontWeight={700} color="white">{vendor.businessName}</Typography>
                <Box sx={{ display: "flex", alignItems: "center", gap: 0.5, mt: 0.5 }}>
                  <LocationOnIcon sx={{ color: "rgba(255,255,255,0.3)", fontSize: 14 }} />
                  <Typography fontSize={13} color="rgba(255,255,255,0.3)">{vendor.address}</Typography>
                </Box>
              </Box>
            </Paper>
          ))}
        </Box>
      )}
    </Box>
  );
}

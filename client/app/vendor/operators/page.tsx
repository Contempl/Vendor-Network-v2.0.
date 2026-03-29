"use client";

import {
  Box, Typography, TextField, Button, Paper, InputAdornment
} from "@mui/material";
import SearchIcon from "@mui/icons-material/Search";
import BusinessIcon from "@mui/icons-material/Business";
import LocationOnIcon from "@mui/icons-material/LocationOn";
import { useState } from "react";
import { getOperators } from "@/entities/vendor/vendor.api";
import { BusinessFrontEndDto } from "@/entities/vendor/vendor-types";

export default function OperatorsPage() {
  const [search, setSearch] = useState("");
  const [operators, setOperators] = useState<BusinessFrontEndDto[]>([]);

  const handleSearch = async () => {
    const result = await getOperators({ name: search });
    setOperators(result);
  };

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", p: 4 }}>
      <Box sx={{ mb: 4 }}>
        <Typography variant="h4" fontWeight={800} color="white" letterSpacing="-1px">
          Поиск операторов
        </Typography>
        <Typography color="rgba(255,255,255,0.3)" fontSize={14} mt={0.5}>
          Найдите операторов для сотрудничества
        </Typography>
      </Box>

      {/* Search */}
      <Box sx={{ display: "flex", gap: 2, mb: 4, maxWidth: 600 }}>
        <TextField
          fullWidth
          placeholder="Название оператора..."
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

      {/* Results */}
      {operators.length === 0 ? (
        <Typography color="rgba(255,255,255,0.2)" fontSize={16} mt={6} textAlign="center">
          Введите название оператора для поиска
        </Typography>
      ) : (
        <Box sx={{ display: "flex", flexDirection: "column", gap: 2, maxWidth: 600 }}>
          {operators.map((op) => (
            <Paper key={op.id} sx={{
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
                <Typography fontWeight={700} color="white">{op.businessName}</Typography>
                <Box sx={{ display: "flex", alignItems: "center", gap: 0.5, mt: 0.5 }}>
                  <LocationOnIcon sx={{ color: "rgba(255,255,255,0.3)", fontSize: 14 }} />
                  <Typography fontSize={13} color="rgba(255,255,255,0.3)">{op.address}</Typography>
                </Box>
              </Box>
            </Paper>
          ))}
        </Box>
      )}
    </Box>
  );
}
"use client";

import { use, useEffect, useState } from "react";
import Link from "next/link";
import { Alert, Box, Chip, CircularProgress, Paper, Typography } from "@mui/material";
import { getFacility } from "@/entities/vendor/vendor.api";
import { VendorFacility } from "@/entities/vendor/vendor-facility-types";

export default function FacilityDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = use(params);
  const facilityId = Number(id);
  const [facility, setFacility] = useState<VendorFacility | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!Number.isInteger(facilityId) || facilityId <= 0) return;
    let active = true;
    getFacility(facilityId)
      .then((result) => { if (active) setFacility(result); })
      .catch(() => { if (active) setError("Could not load this facility."); });
    return () => { active = false; };
  }, [facilityId]);

  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "#0d0d0d", color: "white", p: 4 }}>
      <Link href="/vendor/facilities" style={{ color: "#e94560" }}>Back to facilities</Link>
      {!Number.isInteger(facilityId) || facilityId <= 0 ? (
        <Alert severity="error" sx={{ mt: 3 }}>Invalid facility ID.</Alert>
      ) : error ? (
        <Alert severity="error" sx={{ mt: 3 }}>{error}</Alert>
      ) : !facility ? (
        <CircularProgress sx={{ display: "block", mt: 4, color: "#e94560" }} />
      ) : (
        <Paper sx={{ mt: 3, p: 4, bgcolor: "#161616", color: "white" }}>
          <Typography variant="h4" fontWeight={700}>{facility.name || "Unnamed facility"}</Typography>
          <Typography sx={{ mt: 2 }}>{facility.location}</Typography>
          <Typography sx={{ mt: 1 }}>Service radius: {facility.radiusOfWork} km</Typography>
          <Typography sx={{ mt: 1 }}>Coordinates: {facility.latitude}, {facility.longitude}</Typography>
          <Typography variant="h6" sx={{ mt: 3, mb: 1 }}>Services</Typography>
          {facility.services?.length ? facility.services.map((service) => (
            <Chip key={service.id} label={service.name} sx={{ mr: 1, mb: 1, color: "white", bgcolor: "#383838" }} />
          )) : <Typography color="text.secondary">No services yet.</Typography>}
        </Paper>
      )}
    </Box>
  );
}

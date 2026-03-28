import { create } from "zustand";
import { BusinessFrontEndDto } from "./vendor-types";
import { VendorGetFacilitiesDto } from "./vendor-facility-types";

interface VendorStore {
  vendor: BusinessFrontEndDto | null;
  setVendor: (vendor: BusinessFrontEndDto) => void;
  facilities: VendorGetFacilitiesDto[];
  setFacilities: (facilities: VendorGetFacilitiesDto[]) => void;
}

export const useVendorStore = create<VendorStore>((set) => ({
    vendor: null,
    setVendor: (vendor) => set({ vendor }),
    facilities: [],
    setFacilities: (facilities: VendorGetFacilitiesDto[]) => set({ facilities })
}))
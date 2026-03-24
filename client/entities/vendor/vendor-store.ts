import { create } from "zustand";
import { BusinessFrontEndDto } from "./vendor-types";
import { VendorFacility } from "./vendor-facility-types";

interface VendorStore {
  vendor: BusinessFrontEndDto | null;
  setVendor: (vendor: BusinessFrontEndDto) => void;
  facilities: VendorFacility[];
  setFacilities: (facilities: VendorFacility[]) => void;
}

export const useVendorStore = create<VendorStore>((set) => ({
    vendor: null,
    setVendor: (vendor) => set({ vendor }),
    facilities: [],
    setFacilities: (facilities) => set({ facilities })
}))
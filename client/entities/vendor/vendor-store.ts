import { create } from "zustand";
import { BusinessFrontEndDto } from "./vendor-types";

interface VendorStore {
  vendor: BusinessFrontEndDto | null;
  setVendor: (vendor: BusinessFrontEndDto) => void;
}

export const useVendorStore = create<VendorStore>((set) => ({
    vendor: null,
    setVendor: (vendor) => set({ vendor })
}))
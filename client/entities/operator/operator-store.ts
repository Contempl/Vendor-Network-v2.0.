import { create } from "zustand";
import { BusinessFrontEndDto } from "../vendor/vendor-types";
import { OpIndustryFrontEndDto } from "./operator.types";

interface OperatorStore {
  operator: BusinessFrontEndDto | null;
  setOperator: (operator: BusinessFrontEndDto) => void;
  industries: OpIndustryFrontEndDto[];
  setIndustries: (industries: OpIndustryFrontEndDto[]) => void;
}

export const useOperatorStore = create<OperatorStore>((set) => ({
  operator: null,
  setOperator: (operator) => set({ operator }),
  industries: [],
  setIndustries: (industries) => set({ industries }),
}));

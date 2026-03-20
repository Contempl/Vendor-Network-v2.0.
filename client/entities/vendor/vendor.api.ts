import { BusinessFrontEndDto, EmailForInviteDto, OperatorSearchDto, UpdateVendorDto } from "@/entities/vendor/vendor-types";
import { apiClient } from "../../shared/api/axiosInstance";
import { UpdateVendorFacilityDto, VendorFacility, VendorFacilityDto, VendorFacilityService } from "./vendor-facility-types";

export const getVendor = async (vendorId: number) => {
    const response = await apiClient.get<BusinessFrontEndDto>(`/Vendor/${vendorId}`);
    return response.data;
};

export const updateVendor = async (vendor: UpdateVendorDto) => {
    const response = await apiClient.put<BusinessFrontEndDto>(`/Vendor`, vendor);
    return response.data;
}

export const inviteVendorUser = async (emailDto: EmailForInviteDto) => {
    const response = await apiClient.post('/Vendor/invite', emailDto);
    return response.data;
}

export const getOperators = async (operatorData: OperatorSearchDto) => {
    const response = await apiClient.post<BusinessFrontEndDto[]>('/Vendor/Search/Operators', operatorData);
    return response.data;
}

export const getFacility = async (facilityId: number) => {
    const response = await apiClient.get<VendorFacility>(`/vendor/facility/${facilityId}`);
    return response.data;
}

export const createFacility = async (facilityData: VendorFacilityDto) => {
    const response = await apiClient.post<VendorFacility>('/vendor/facility', facilityData);
    return response.data;
}

export const updateFacility = async (facilityId: number, facilityData: UpdateVendorFacilityDto) => {
    const response = await apiClient.put<VendorFacility>(`/vendor/facility/${facilityId}`, facilityData);
    return response.data;
}

export const deleteFacility = async (vendorId: number, facilityId: number) => {
    const response = await apiClient.delete(`/vendor/${vendorId}/facilities/${facilityId}`);
    return response.data;
}

export const getFacilityServices = async (facilityId: number) => {
    const response = await apiClient.get<VendorFacilityService[]>(`/vendor/facility/${facilityId}/services`);
    return response.data;
}
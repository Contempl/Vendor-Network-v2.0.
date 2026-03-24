import { BusinessFrontEndDto, EmailForInviteDto, OperatorSearchDto, UpdateVendorDto } from "@/entities/vendor/vendor-types";
import { apiClient } from "../../shared/api/axiosInstance";

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
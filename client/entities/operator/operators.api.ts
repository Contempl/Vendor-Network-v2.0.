import { apiClient } from "@/shared/api/axiosInstance";
import { OperatorIndustryCreationDto, OpIndustryFrontEndDto, SearchVendorsForIndustriesDto, UpdateOperatorDto, VendorSearchDto } from "./operator.types";
import { BusinessFrontEndDto } from "../vendor/vendor-types";
import { getBusinessIdFromToken } from "../auth/auth-utils";


export async function getVendors(vendorSearchDto: VendorSearchDto) {
    const response = await apiClient.post("Operator/search/vendor", vendorSearchDto);
    return response.data;
}

export async function searchVendorsToServeFacilities(searchDto: SearchVendorsForIndustriesDto) {
    const response = await apiClient.post<BusinessFrontEndDto[]>("Operator/search/vendors", searchDto);
    return response.data;
}

export async function getOperatorIndustries() {
    const response = await apiClient.get<OpIndustryFrontEndDto[]>("Operator/industries");
    return response.data;
}

export async function createOperatorIndustry(industryCreationDto: OperatorIndustryCreationDto) {
    const operatorId = getBusinessIdFromToken();
    const response = await apiClient.post<OpIndustryFrontEndDto>(`/operator/${operatorId}/industry`, industryCreationDto);
    return response.data;
}
export async function deleteOperatorIndustry(industryId: number) {
    const response = await apiClient.delete<number>(`Operator/industry/${industryId}`);
    return response.data;
}

export async function updateOperator(updateDto: UpdateOperatorDto) {
    const response = await apiClient.put<BusinessFrontEndDto>("Operator", updateDto);
    return response.data;
}

export async function getOperator() {
    const response = await apiClient.get<BusinessFrontEndDto>("Operator");
    return response.data;
}

export async function inviteUserToOperator(email: string) {
    const response = await apiClient.post("Operator/invite", { email });
    return response.data;
}
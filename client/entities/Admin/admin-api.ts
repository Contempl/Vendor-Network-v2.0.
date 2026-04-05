import { apiClient } from "@/shared/api/axiosInstance";
import { BusinessInvitationData, DataForInviteDto } from "../auth/auth-types";


export async function inviteBusiness(data: BusinessInvitationData){
    const response = await apiClient.post("/Admin/inviteBusiness", data);
    return response.data;
}

export async function inviteVendorUser(data: DataForInviteDto){
    const response = await apiClient.post("/inviteVendorUser", data);
    return response.data;
}


export async function inviteOperatorUser(data: DataForInviteDto){
    const response = await apiClient.post("/inviteOperatorUser", data);
    return response.data;
}

export async function removeVendor(vendorId: number) {
    const response = await apiClient.delete(`/Admin/Vendor/${vendorId}`);
    return response.data;
}

export async function removeOperator(operatorId: number) {
    const response = await apiClient.delete(`/Admin/Operator/${operatorId}`);
    return response.data;
}
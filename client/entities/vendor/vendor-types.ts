export interface BusinessFrontEndDto {
    id: number;
    businessName: string;
    address: string;
    occupation?: string | null;
}

export interface UpdateVendorDto {
    businessName: string | null;
    address: string | null;
    email: string | null;
}

export interface EmailForInviteDto {
    email: string;
}

export interface OperatorSearchDto {
    name: string;
}


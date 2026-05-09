export interface UpdateOperatorDto {
    businessName: string | null;
    address: string | null;
    email: string | null;
    logoUrl: string | null;
    occupation: string | null;
}

export interface OpIndustryFrontEndDto {
    id: number;
    address: string;
    name: string;
}

export interface OperatorIndustryCreationDto {
    name: string;
    address: string;
    latitude: number;
    longitude: number;
}

export interface SearchVendorsForIndustriesDto {
    serviceType: string;
    industriesLocationIds: number[];
}

export enum SortOrder {
    Ascending = 0,
    Descending = 1
}

export interface VendorSearchDto {
    vendorName: string;
    sortOrder: SortOrder;
    pageSize: number;
    pageNumber: number;
}
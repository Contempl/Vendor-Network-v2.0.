export interface VendorFacility {
    id: number;
    name: string | null;
    location: string;
    longitude: number;
    latitude: number;
    radiusOfWork: number;
    services: VendorFacilityService[];
}

export interface VendorFacilityService {
    id: number;
    name: string;
    vendorFacilityId: number;
}

export interface UpdateVendorFacilityDto {
    name: string | null;
    location: string | null;
    longitude: number | null;
    latitude: number | null;
    radiusOfWork: number | null;
    services: string[] | null;
}


export interface VendorFacilityDto {
    name: string;
    location: string;
    longitude: number;
    latitude: number;
    radiusOfWork: number;
    services: string[] | null;
}
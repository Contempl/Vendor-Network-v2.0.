namespace Product.Domain.Enum;

public enum ErrorCodes
{
    // 0 - 10 - Invites
    // 11- 20 - User
    // 21 - 30 - Business
    // 31 - 40 - Operator Industries
    // 41 - 50 - Vendor Facilities
    // 51 - 60 - Tokens
    
    InvalidInvitation = 1,
    
    UserNotFound = 11,
    InvalidPassword = 12,
    InvalidInvitationData = 13,
    UsersDontMatch = 14,
    UserWithThisEmailAlreadyExists = 15,
    
    InvalidServiceType = 21,
    InvalidBusinessName = 22,
    InvalidBusinessRegistrationData = 23,
    
    InvalidOperatorIndustryData = 31,
    OperatorIndustryNotFound = 32,
    
    InvalidVendorFacilityData = 41,
    InvalidVendorFacilityServiceData = 42,
    
    InvalidRefreshToken = 51,
}
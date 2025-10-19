using Product.Application.Dto;
using Product.Domain.Dto;
using Product.Domain.Entity;

namespace Product.Application.Mapping;

public static class BusinessMappingExtension
{
    public static BusinessFrontEndDto ToFrontEndDto(this Business business)
    {
        return new BusinessFrontEndDto
        {
            Id = business.Id,
            BusinessName = business.BusinessName,
            Address = business.Address
        };
    }

    public static void MapVendorToUpdate(this Business vendor, UpdateVendorDto vendorData)
    {
        vendor.BusinessName = vendorData.BusinessName ?? vendor.BusinessName;
        vendor.Address = vendorData.Address ?? vendor.Address;
        vendor.Email = vendorData.Email ?? vendor.Email;
    }
}
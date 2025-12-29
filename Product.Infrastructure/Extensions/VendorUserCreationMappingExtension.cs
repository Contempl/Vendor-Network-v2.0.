using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Infrastructure.Extensions;

public static class VendorUserCreationMappingExtension
{
    public static VendorUser MapToVendorUser(this VendorUserCreationDto vendorUserCreationDto) =>
        new VendorUser
        {
            Email = vendorUserCreationDto.Email,
            VendorId = vendorUserCreationDto.VendorId,
            UserType = vendorUserCreationDto.UserType,
            CreatedBy = vendorUserCreationDto.CreatedBy
        };
}
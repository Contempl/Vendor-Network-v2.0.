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
}
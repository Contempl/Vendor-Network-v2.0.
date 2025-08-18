using Product.Domain.Dto;
using Product.Domain.Entity;

namespace Product.Application.Mapping;

public static class OperatorIndustryMappingExtension
{
    public static OpIndustryFrontEndDto ToFrontEndDto(this OperatorIndustry operatorIndustry)
    {
        return new OpIndustryFrontEndDto
        {
            Id = operatorIndustry.Id,
            Name = operatorIndustry.Name,
            Address = operatorIndustry.Address
        };
    }
}
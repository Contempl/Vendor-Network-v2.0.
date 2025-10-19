using Product.Application.Dto;
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
    
    public static void MapIndustryToUpdate(this OperatorIndustry industry, 
        UpdateOperatorIndustryDto industryData)
    {
        industry.Name = industryData.Name ?? industry.Name;
        industry.Address = industryData.Address ?? industry.Address;
        industry.Latitude = industryData.Latitude ?? industry.Latitude;
        industry.Longitude = industryData.Longitude ?? industry.Longitude;
    }
}
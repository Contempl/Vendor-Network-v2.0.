using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.Mapping;

public static class OperatorMappingExtension
{
    public static void MapOperatorFromDtoToUpdate(this Operator @operator, UpdateOperatorDto operatorData)
    {
        @operator.BusinessName = operatorData.BusinessName ?? @operator.BusinessName;
        @operator.Address = operatorData.Address ?? @operator.Address;
        @operator.Email = operatorData.Email ?? @operator.Email;
        @operator.LogoUrl = operatorData.LogoUrl ?? @operator.LogoUrl;
        @operator.Occupation = operatorData.Occupation ?? @operator.Occupation;
    }
}
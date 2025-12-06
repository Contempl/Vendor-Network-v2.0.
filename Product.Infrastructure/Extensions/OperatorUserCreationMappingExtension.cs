using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Infrastructure.Extensions;

public static class OperatorUserCreationMappingExtension
{
    public static OperatorUser MapToOperatorUser(this OperatorUserCreationDto operatorUserCreationDto) =>
        new OperatorUser
        {
            Email = operatorUserCreationDto.Email,
            OperatorId = operatorUserCreationDto.OperatorId,
            UserType = operatorUserCreationDto.UserType,
            CreatedBy = operatorUserCreationDto.CreatedBy,
        };
}
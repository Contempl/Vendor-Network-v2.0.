using Product.Domain.Dto;
using Product.Domain.Entity;

namespace Product.Application.Mapping;

public static class AdministratorMappingExtensions
{
    public static UserClaimDto MapAdminToClaimDto(this Administrator admin)
    {
        return new UserClaimDto()
        {
            Id = admin.Id,
            Email = admin.Email,
            UserType = admin.UserType,
        };
    }
}
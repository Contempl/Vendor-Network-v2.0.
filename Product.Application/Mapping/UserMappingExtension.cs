using Product.Domain.Dto;
using Product.Domain.Entity;

namespace Product.Application.Mapping;

public static class UserMappingExtensions
{
    public static UserDto MapToDto(this User user)
    {
        return new UserDto
        {
            Login = user.UserName
        };
    }

    public static List<UserDto> ToDtoList(this IEnumerable<User> users)
    {
        return users.Select(u => u.MapToDto()).ToList();
    }

    public static UserDtoToFrontEnd MapToFrontEndDto(this User user)
    {
        return new UserDtoToFrontEnd
        {
            Id = user.Id,
            FirstName = user.FirstName,
            Email = user.Email
        };
    }
    
    public static void MapUserToUpdate(this User user, UserToUpdateDto userUpdateData)
    {
        user.UserName = userUpdateData.UserName ?? user.UserName;
        user.FirstName = userUpdateData.FirstName ?? user.FirstName;
        user.LastName = userUpdateData.LastName ?? user.LastName;
        user.Email = userUpdateData.Email ?? user.Email;
    }
    
    public static UserClaimDto MapUserToClaimDto(this User user)
    {
        var dto = new UserClaimDto();
        switch (user)
        {
            case OperatorUser operatorUser:
                dto.Id = operatorUser.Id;
                dto.Email = operatorUser.Email;
                dto.UserType = operatorUser.UserType;
                dto.BusinessId = operatorUser.OperatorId!.Value;
                break;
            case VendorUser vendorUser:
                dto.Id = vendorUser.Id;
                dto.Email = vendorUser.Email;
                dto.UserType = vendorUser.UserType;
                dto.BusinessId = vendorUser.VendorId!.Value;
                break;
            default :
                dto.Id = user.Id;
                dto.Email = user.Email;
                dto.UserType = user.UserType;
                break;
        }
		
        return dto;
    }
}
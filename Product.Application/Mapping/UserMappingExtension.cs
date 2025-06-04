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
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.Mapping;
using Product.Application.ServiceInterfaces;
using Product.Domain.Dto;
using Product.Domain.Entity;
using Product.Domain.Enum;
using Product.Domain.Pagination;
using Product.Domain.Result;

namespace Product.Infrastructure.Implementations;

public class OperatorService : IOperatorService
{
    private readonly IOperatorRepository _operatorRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IOperatorIndustryRepository _operatorFacilityRepository;
    private readonly IOperatorUserRepository _operatorUserRepository;
    private readonly IEmailService _emailService;
    private readonly IInviteRepository _inviteRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserPrincipalService _userPrincipalService;
    private readonly IInviteService _inviteService;

	public OperatorService(IOperatorRepository operatorRepository, 
        IVendorRepository vendorRepository, IOperatorIndustryRepository operatorFacilityRepository, IUserRepository userRepository, IOperatorUserRepository operatorUserRepository, IEmailService emailService, IInviteRepository inviteRepository, IUserPrincipalService userPrincipalService, IInviteService inviteService)
	{
		_operatorRepository = operatorRepository;
        _vendorRepository = vendorRepository;
		_operatorFacilityRepository = operatorFacilityRepository;
        _userRepository = userRepository;
        _operatorUserRepository = operatorUserRepository;
        _emailService = emailService;
        _inviteRepository = inviteRepository;
        _userPrincipalService = userPrincipalService;
        _inviteService = inviteService;
    }


    private List<OperatorIndustry> GetAllOperatorIndustries(List<int> facilityIds)
    {
        if (facilityIds.Count == 0)
        {
            throw new ArgumentException("No facilities provided for search");
        }
        var facilities = _operatorFacilityRepository.GetAll()
            .Where(of => facilityIds.Contains(of.Id)).ToList();

        return facilities;
	}
    private bool ValidateStringInput(string input)
    {
        return !string.IsNullOrWhiteSpace(input);
    }

    private Operator MapOperatorFromDto(OperatorRegistrationDto operatorRegistrationData, 
        OperatorUser user) => new Operator
    {
		BusinessName = operatorRegistrationData.BusinessName,
		Address = operatorRegistrationData.Address,
		Email = operatorRegistrationData.Email,
		LogoUrl = operatorRegistrationData.LogoUrl,
		Occupation = operatorRegistrationData.Occupation,
		OperatorUsers = new List<OperatorUser> { user }
	};

    private void MapOperatorFromDtoToUpdate(Operator @operator, UpdateOperatorDto operatorData)
    {
		@operator.BusinessName = operatorData.BusinessName ?? @operator.BusinessName;
		@operator.Address = operatorData.Address ?? @operator.Address;
		@operator.Email = operatorData.Email ?? @operator.Email;
		@operator.LogoUrl = operatorData.LogoUrl ?? @operator.LogoUrl;
		@operator.Occupation = operatorData.Occupation ?? @operator.Occupation;
	}

    private async Task<List<Vendor>> SearchVendorsAsync(string serviceType, List<OperatorIndustry> operatorFacilities)
    {
        if (operatorFacilities == null || !operatorFacilities.Any())
        {
            return new List<Vendor>();
        }

        var vendorsWithService = await _vendorRepository.GetVendorsWithService(serviceType);

        var matchingVendors = vendorsWithService.Where(vendor =>
            vendor.VendorFacilities.Any(facility =>
                operatorFacilities.All(operatorFacility =>
                    IsOperatorIndustryInServiceArea(operatorFacility, facility)))).ToList();
		
        return matchingVendors;
    }

    public async Task<Response<List<BusinessFrontEndDto>>> SearchForVendorsAsync(SearchVendorsForIndustriesDto industriesData)
    {
        var serviceIsValid = ValidateStringInput(industriesData.ServiceType);
        if (!serviceIsValid)
        {
            return new Response<List<BusinessFrontEndDto>>()
            {
                ErrorMessage = $"The field '{nameof(industriesData.ServiceType)}' should't be empty ",
                ErrorCode = (int)ErrorCodes.InvalidServiceType,
            };
        }

        var operatorFacilities = GetAllOperatorIndustries(industriesData.IndustriesLocationIds);

        var vendors = await SearchVendorsAsync(industriesData.ServiceType, operatorFacilities);
        
        var vendorDtos = vendors.Select(v => v.ToFrontEndDto()).ToList();

        return new Response<List<BusinessFrontEndDto>>
        {
            Data = vendorDtos
        };
    }

    private double CalculateDistance(double operLat, double operLon, double vendLat, double vendLon)
    {
        const double R = 6371;

        var dLat = ToRadians(vendLat - operLat);
        var dLon = ToRadians(vendLon - operLon);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(operLat)) * Math.Cos(ToRadians(vendLat)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        var distance = R * c;
        return distance;
    }
    private double ToRadians(double degrees)
    {
        return (Math.PI / 180) * degrees;
    }
    private bool IsOperatorIndustryInServiceArea(OperatorIndustry operatorFacility, VendorFacility vendorFacility)
    {
        double distance = CalculateDistance(operatorFacility.Latitude, operatorFacility.Longitude,
            vendorFacility.Latitude, vendorFacility.Longitude);
        return distance <= vendorFacility.RadiusOfWork;
    }

    public async Task<Response<PagedList<Vendor>>> GetVendorsByNameAsync(VendorSearchDto vendorSearchDto)
    {
        var isValidVendor = ValidateStringInput(vendorSearchDto.VendorName);
        if (!isValidVendor)
        {
            return new Response<PagedList<Vendor>>
            {
                ErrorMessage = $"The field '{nameof(vendorSearchDto.VendorName)}' shouldn't be empty",
                ErrorCode = (int)ErrorCodes.InvalidBusinessName
            };
        }

        var pagedVendors = await _vendorRepository.GetVendorsQuery(vendorSearchDto.VendorName, vendorSearchDto.SortOrder,
            vendorSearchDto.PageSize, vendorSearchDto.PageNumber);

        var result = new PagedList<Vendor>
            (pagedVendors.Items, vendorSearchDto.PageSize, vendorSearchDto.PageNumber, pagedVendors.TotalCount);

        return new Response<PagedList<Vendor>>
        {
            Data = result
        };
    }

    public async Task<Response<BusinessFrontEndDto>> GetOperatorAsync(int operatorId)
    {
        var @operator = await _operatorRepository.GetByIdAsync(operatorId);
        var operatorDto = @operator.ToFrontEndDto();

        return new Response<BusinessFrontEndDto>
        {
            Data = operatorDto
        };
    }

    public async Task<Response<BusinessFrontEndDto>> RegisterOperatorAsync(int operatorUserId, OperatorRegistrationDto operatorRegistrationData)
    {
        var user = await _operatorUserRepository.GetByIdAsync(operatorUserId);

        var newOperator = MapOperatorFromDto(operatorRegistrationData, user);

        await _operatorRepository.CreateAsync(newOperator);
        var businessDto = newOperator.ToFrontEndDto();
        return new Response<BusinessFrontEndDto>
        {
            Data = businessDto
        };
    }

    public async Task<Response<MailMsg>> InviteOperatorUserAsync(int operatorUserId, EmailForInviteDto dto)
    {
        var operatorUser = await _userRepository.GetByIdAsync(operatorUserId);
		
        var operatorId = _userPrincipalService.BusinessId;
		
        var newOperatorUser = new OperatorUser { Email = dto.Email, OperatorId = operatorId };
		
        await _operatorUserRepository.CreateAsync(newOperatorUser);
		
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

        if (existingUser == null)
            return new Response<MailMsg>
            {
                ErrorMessage = $"User with email {dto.Email} was not found",
                ErrorCode = (int)ErrorCodes.UserNotFound
            };
		
        var invite =  _inviteService.CreateInvite(existingUser, operatorUser);
        var inviteUrl = _emailService.CreateInviteUrl(invite.Id); 
        await _inviteRepository.CreateAsync(invite);
		
        var emailBody = _emailService.GenerateEmailTemplate(dto.Email, existingUser, inviteUrl);

        var mailMessage = _emailService.CreateMessage(emailBody, operatorUser.Email);

        await _emailService.SendInvitationEmailAsync(mailMessage);

        return new Response<MailMsg>
        {
            Data = mailMessage
        };
    }

    public async Task<Response<int>> DeleteOperatorAsync(int operatorId)
    {
        var @operator = await _operatorRepository.GetByIdAsync(operatorId);
        await _operatorRepository.DeleteAsync(@operator);
        return new Response<int>
        {
            Data = @operator.Id
        };
    }
    public async Task<Response<BusinessFrontEndDto>> UpdateOperatorAsync(int operatorId, UpdateOperatorDto operatorUpdateData)
    {
        var @operator = await _operatorRepository.GetByIdAsync(operatorId);
        MapOperatorFromDtoToUpdate(@operator, operatorUpdateData);

        await _operatorRepository.UpdateAsync(@operator);
        return new Response<BusinessFrontEndDto>
        {
            Data = @operator.ToFrontEndDto()
        };
    }
}   

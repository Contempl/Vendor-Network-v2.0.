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
    private readonly IUnitOfWork _unitOfWork;

	public OperatorService
    (
        IOperatorRepository operatorRepository, IVendorRepository vendorRepository, IOperatorIndustryRepository operatorFacilityRepository, 
        IUserRepository userRepository, IOperatorUserRepository operatorUserRepository, IEmailService emailService, IInviteRepository inviteRepository, 
        IUserPrincipalService userPrincipalService, IInviteService inviteService, IUnitOfWork unitOfWork)
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
        _unitOfWork = unitOfWork;
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
    
    private void MapOperatorFromDtoToUpdate(Operator @operator, UpdateOperatorDto operatorData)
    {
		@operator.BusinessName = operatorData.BusinessName ?? @operator.BusinessName;
		@operator.Address = operatorData.Address ?? @operator.Address;
		@operator.Email = operatorData.Email ?? @operator.Email;
		@operator.LogoUrl = operatorData.LogoUrl ?? @operator.LogoUrl;
		@operator.Occupation = operatorData.Occupation ?? @operator.Occupation;
	}

    private async Task<List<Vendor>> SearchVendorsAsync(string serviceType, List<OperatorIndustry> operatorFacilities, CancellationToken cancellationToken)
    {
        if (operatorFacilities == null || !operatorFacilities.Any())
        {
            return new List<Vendor>();
        }

        var vendorsWithService = await _vendorRepository.GetVendorsWithService(serviceType, cancellationToken);

        var matchingVendors = vendorsWithService.Where(vendor =>
            vendor.VendorFacilities.Any(facility =>
                operatorFacilities.All(operatorFacility =>
                    IsOperatorIndustryInServiceArea(operatorFacility, facility)))).ToList();
		
        return matchingVendors;
    }

    public async Task<Response<List<BusinessFrontEndDto>>> SearchForVendorsAsync(SearchVendorsForIndustriesDto industriesData, 
        CancellationToken cancellationToken)
    {
        var serviceIsValid = ValidateStringInput(industriesData.ServiceType);
        if (!serviceIsValid)
        {
            return new Response<List<BusinessFrontEndDto>>()
            {
                ErrorMessage = $"The field '{nameof(industriesData.ServiceType)}' shouldn't be empty ",
                ErrorCode = (int)ErrorCodes.InvalidServiceType,
            };
        }

        var operatorFacilities = GetAllOperatorIndustries(industriesData.IndustriesLocationIds);

        var vendors = await SearchVendorsAsync(industriesData.ServiceType, operatorFacilities, cancellationToken);
        
        var vendorDtos = vendors.Select(v => v.ToFrontEndDto()).ToList();

        return new Response<List<BusinessFrontEndDto>>
        {
            Data = vendorDtos
        };
    }

    private double CalculateDistance(double operLatitude, double operLongitude, double vendLatitude, double vendLongitude)
    {
        const double R = 6371;

        var dLat = ToRadians(vendLatitude - operLatitude);
        var dLon = ToRadians(vendLongitude - operLongitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(operLatitude)) * Math.Cos(ToRadians(vendLatitude)) *
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

    public async Task<Response<PagedList<Vendor>>> GetVendorsByNameAsync(VendorSearchDto vendorSearchDto,
        CancellationToken cancellationToken)
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
            vendorSearchDto.PageSize, vendorSearchDto.PageNumber, cancellationToken);

        var result = new PagedList<Vendor>
            (pagedVendors.Items, vendorSearchDto.PageSize, vendorSearchDto.PageNumber, pagedVendors.TotalCount);

        return new Response<PagedList<Vendor>>
        {
            Data = result
        };
    }

    public async Task<Response<BusinessFrontEndDto>> GetOperatorAsync(int operatorId, 
        CancellationToken cancellationToken)
    {
        var @operator = await _operatorRepository.GetByIdAsync(operatorId, cancellationToken);
        var operatorDto = @operator.ToFrontEndDto();

        return new Response<BusinessFrontEndDto>
        {
            Data = operatorDto
        };
    }

    public async Task<Response<MailMsg>> InviteOperatorUserAsync(EmailForInviteDto dto, 
        CancellationToken cancellationToken)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var operatorUserId = _userPrincipalService.UserId!.Value;
            var operatorId = _userPrincipalService.BusinessId;
            
            var operatorUser = await _userRepository.GetByIdAsync(operatorUserId, cancellationToken);

            var newOperatorUser = new OperatorUser
                { Email = dto.Email, OperatorId = operatorId, UserType = UserType.OperatorUser };

            await _operatorUserRepository.CreateAsync(newOperatorUser, cancellationToken);

            var invite = _inviteService.CreateInvite(newOperatorUser, operatorUser);
            var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
            await _inviteRepository.CreateAsync(invite, cancellationToken);
            var emailBody = _emailService.GenerateEmailTemplate(dto.Email, newOperatorUser, inviteUrl);

            var mailMessage = _emailService.CreateMessage(emailBody, operatorUser.Email);
            await _emailService.SendInvitationEmailAsync(mailMessage);

            await transaction.CommitAsync(cancellationToken);

            return new Response<MailMsg>
            {
                Data = mailMessage
            };
        }

        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);

            return new Response<MailMsg>
            {
                ErrorCode = (int)ErrorCodes.InvalidInvitationData,
                ErrorMessage = "Failed to craete an invite in transaction"
            };
        }
    }
    public Task<Response<BusinessFrontEndDto>> UpdateOperatorAsync(UpdateOperatorDto operatorUpdateData, CancellationToken cancellationToken)
    {
        var operatorId = _userPrincipalService.BusinessId!.Value;
        var @operator = _operatorRepository.GetByIdAsync(operatorId, cancellationToken).Result;
        MapOperatorFromDtoToUpdate(@operator, operatorUpdateData);

        _operatorRepository.UpdateAsync(@operator, cancellationToken);
        return Task.FromResult(new Response<BusinessFrontEndDto>
        {
            Data = @operator.ToFrontEndDto()
        });
    }
}   

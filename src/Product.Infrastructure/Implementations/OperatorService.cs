using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
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
    private readonly ILogger<OperatorService> _logger;

	public OperatorService(
        IOperatorRepository operatorRepository, 
        IVendorRepository vendorRepository, 
        IOperatorIndustryRepository operatorFacilityRepository, 
        IUserRepository userRepository, 
        IOperatorUserRepository operatorUserRepository, 
        IEmailService emailService, 
        IInviteRepository inviteRepository, 
        IUserPrincipalService userPrincipalService, 
        IInviteService inviteService, 
        IUnitOfWork unitOfWork, 
        ILogger<OperatorService> logger)
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
        _logger = logger;
    }



    public async Task<OneOf<List<BusinessFrontEndDto>, FacilityNotFound, ValidationError, Error>> SearchForVendorsAsync(SearchVendorsForIndustriesDto industriesData, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serviceIsValid = ValidateStringInput(industriesData.ServiceType);
            if (!serviceIsValid)
            {
                _logger.LogError($"Invalid Service Type: {industriesData.ServiceType}.");
                return new ValidationError();
            }

            var operatorFacilitiesResult = GetAllOperatorIndustries(industriesData.IndustriesLocationIds);
            if (operatorFacilitiesResult.Value is FacilityNotFound facilityNotFound)
                return facilityNotFound;
            
            var operatorFacilities = operatorFacilitiesResult.Value as List<OperatorIndustry>;

            var vendors = await SearchVendorsAsync(industriesData.ServiceType, operatorFacilities!, cancellationToken);
        
            var vendorDtoList = vendors.Select(v => v.ToFrontEndDto()).ToList();

            return vendorDtoList;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to search for vendors with exception.");
            return new Error();
        }
    }
    
    public async Task<OneOf<PagedList<Vendor>, ValidationError>> GetVendorsByNameAsync(VendorSearchDto vendorSearchDto,
        CancellationToken cancellationToken = default)
    {
        var isValidVendor = ValidateStringInput(vendorSearchDto.VendorName);
        if (!isValidVendor)
        {
            _logger.LogError($"Invalid Vendor Name: {vendorSearchDto.VendorName}.");
            return new ValidationError();
        }

        var pagedVendors = await _vendorRepository.GetVendorsQuery(vendorSearchDto.VendorName, vendorSearchDto.SortOrder,
            vendorSearchDto.PageSize, vendorSearchDto.PageNumber, cancellationToken);

        var result = new PagedList<Vendor>
            (pagedVendors.Items, vendorSearchDto.PageSize, vendorSearchDto.PageNumber, pagedVendors.TotalCount);

        return result;
    }

    public async Task<OneOf<BusinessFrontEndDto, Error>> GetOperatorAsync(int operatorId, 
        CancellationToken cancellationToken = default)
    {
        var @operator = await _operatorRepository.GetByIdAsync(operatorId, cancellationToken);
        var operatorDto = @operator.ToFrontEndDto();

        return operatorDto;
    }

    public async Task<OneOf<MailMsg, Error>> InviteOperatorUserAsync(EmailForInviteDto dto, 
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var operatorUserId = _userPrincipalService.UserId!.Value;
            var operatorId = _userPrincipalService.BusinessId;
            
            var operatorUser = await _userRepository.GetByIdAsync(operatorUserId, cancellationToken);

            var newOperatorUser = new OperatorUser
            {
                Email = dto.Email, 
                OperatorId = operatorId, 
                UserType = UserType.OperatorUser, 
                CreatedBy = operatorUserId
            };

            await _operatorUserRepository.CreateAsync(newOperatorUser, cancellationToken);

            var invite = _inviteService.CreateInvite(newOperatorUser, operatorUser);
            var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
            await _inviteRepository.CreateAsync(invite, cancellationToken);
            var emailBody = _emailService.GenerateEmailTemplate(dto.Email, newOperatorUser, inviteUrl);

            var mailMessage = _emailService.CreateMessage(emailBody, operatorUser.Email);
            await _emailService.SendInvitationEmailAsync(mailMessage);

            await transaction.CommitAsync(cancellationToken);

            return mailMessage;
        }

        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex ,"Couldn't create operator invitation.");
            return new Error();
        }
    }
    public async Task<OneOf<BusinessFrontEndDto, Error>> UpdateOperatorAsync(UpdateOperatorDto operatorUpdateData, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var operatorId = _userPrincipalService.BusinessId!.Value;
            var @operator = await _operatorRepository.GetByIdAsync(operatorId, cancellationToken);
            @operator.MapOperatorFromDtoToUpdate(operatorUpdateData);

            await _operatorRepository.UpdateAsync(@operator, cancellationToken);
            
            var operatorDto = @operator.ToFrontEndDto();
            return operatorDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't update the operator.");
            return new Error();
        }
    }
    
    private async Task<List<Vendor>> SearchVendorsAsync(string serviceType, List<OperatorIndustry> operatorFacilities, 
        CancellationToken cancellationToken = default)
    {
        if (operatorFacilities == null || !operatorFacilities.Any())
        {
            _logger.LogError("No operator facilities provided.");
            return new List<Vendor>();
        }

        var vendorsWithService = await _vendorRepository.GetVendorsWithService(serviceType, cancellationToken);

        var matchingVendors = vendorsWithService.Where(vendor =>
            vendor.VendorFacilities.Any(facility =>
                operatorFacilities.All(operatorFacility =>
                    IsOperatorIndustryInServiceArea(operatorFacility, facility)))).ToList();
		
        return matchingVendors;
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
    
    private OneOf<FacilityNotFound, List<OperatorIndustry>> GetAllOperatorIndustries(List<int> facilityIds)
    {
        if (facilityIds.Count == 0)
        {
            _logger.LogWarning("No facilities found.");
            return new FacilityNotFound("");
        }
        var facilities = _operatorFacilityRepository.GetAll()
            .Where(of => facilityIds.Contains(of.Id)).ToList();

        return facilities;
    }
    private bool ValidateStringInput(string input)
    {
        return !string.IsNullOrWhiteSpace(input);
    }
}   

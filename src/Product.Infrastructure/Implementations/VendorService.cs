using Microsoft.EntityFrameworkCore;
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
using Product.Domain.Result;

namespace Product.Infrastructure.Implementations;

public class VendorService : IVendorService
{
    private readonly IVendorRepository _vendorRepository;
    private readonly IOperatorRepository _operatorRepository;
    private readonly IVendorUserRepository _vendorUserRepository;
    private readonly IUserPrincipalService _userPrincipalService;
    private readonly IEmailService _emailService;
    private readonly IInviteService _inviteService;
    private readonly IInviteRepository _inviteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<VendorService> _logger;

    public VendorService(IVendorRepository vendorRepository,
        IOperatorRepository operatorRepository,
        IVendorUserRepository vendorUserRepository,
        IUserPrincipalService userPrincipalService,
        IEmailService emailService,
        IInviteService inviteService, 
        IInviteRepository inviteRepository,
        IUnitOfWork unitOfWork,
        ILogger<VendorService> logger)
    {
        _vendorRepository = vendorRepository;
        _operatorRepository = operatorRepository;
        _vendorUserRepository = vendorUserRepository;
        _userPrincipalService = userPrincipalService;
        _emailService = emailService;
        _inviteService = inviteService;
        _inviteRepository = inviteRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<OneOf<List<BusinessFrontEndDto>, InvalidOperatorNameError, Error>> SearchOperatorsAsync(OperatorSearchDto operatorSearchDto, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(operatorSearchDto.Name))
            {
                _logger.LogWarning("Operator name for search is invalid");
                return new InvalidOperatorNameError("Invalid Operator Name");
            }

            var operators = await _operatorRepository.GetOperatorsByNameAsync(operatorSearchDto.Name, cancellationToken);
            var result = operators.Select(o => o.ToFrontEndDto()).ToList();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search for operators failed");
            return new Error();
        }
    }

    public async Task<OneOf<BusinessFrontEndDto, Error>> GetVendorByIdAsync(int vendorId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);
            var result = vendor.ToFrontEndDto();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while getting vendor");
            return new Error();
        }
    }

    public async Task<OneOf<BusinessFrontEndDto, Error>> UpdateVendorAsync(UpdateVendorDto vendorData, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var vendorId = _userPrincipalService.BusinessId!.Value;
            var existingVendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);

            existingVendor.MapVendorToUpdate(vendorData);

            await _vendorRepository.UpdateAsync(existingVendor, cancellationToken);

            var result = existingVendor.ToFrontEndDto();
        
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while updating vendor");
            return new Error();
        }
    }

    public async Task<OneOf<MailMsg, Error>> InviteVendorUserAsync(EmailForInviteDto emailDto, 
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var vendorUserId = _userPrincipalService.UserId!.Value;
            var vendorId = _userPrincipalService.BusinessId;
            var vendorUser = await _vendorUserRepository.GetByIdAsync(vendorUserId, cancellationToken);

            var email = emailDto.Email;

            var newVendorUser = new VendorUser
            {
                Email = email, 
                VendorId = vendorId, 
                UserType = UserType.VendorUser, 
                CreatedBy = vendorUserId
            };

            await _vendorUserRepository.CreateAsync(newVendorUser, cancellationToken);

            var invite = _inviteService.CreateInvite(newVendorUser, vendorUser);
            var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
            await _inviteRepository.CreateAsync(invite, cancellationToken);

            var emailBody = _emailService.GenerateEmailTemplate(email, newVendorUser, inviteUrl);

            var mailMessage = _emailService.CreateMessage(emailBody, vendorUser.Email);

            await _emailService.SendInvitationEmailAsync(mailMessage);

            await transaction.CommitAsync(cancellationToken);

            return mailMessage;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            
            _logger.LogError(ex, "Failed to create an invite for vendor user");
            return new Error();
        }
    }

    public async Task<OneOf<List<VendorGetFacilitiesDto>, Error>> GetVendorFacilitiesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var vendorId = _userPrincipalService.BusinessId;
            if (vendorId is null)
            {
                _logger.LogInformation("Vendor Id missing in current context.");
                return new Error();
            }

            var vendorFacilities = await _vendorRepository
                .GetVendorFacilitiesAsync(vendorId.Value, cancellationToken);

            var resultFacilities = vendorFacilities.Select(vf => new VendorGetFacilitiesDto
            {
                Id = vf.Id,
                Name = vf.Name ?? "unknown facility name",
                Location = vf.Location,
                Longitude = vf.Longitude,
                Latitude = vf.Latitude,
                RadiusOfWork = vf.RadiusOfWork,
                Services = vf.Services.Select(s => s.Name).ToList(),
            }).ToList();

            return resultFacilities;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error while fetching vendor facility. {ex}", ex);
            return  new Error();
        }
    }
}
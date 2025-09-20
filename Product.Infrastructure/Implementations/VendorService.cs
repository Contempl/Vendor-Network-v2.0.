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
    private readonly IUserRepository _userRepository;
    private readonly IInviteRepository _inviteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VendorService(IVendorRepository vendorRepository, IOperatorRepository operatorRepository,
        IVendorUserRepository vendorUserRepository, IUserPrincipalService userPrincipalService,
        IEmailService emailService, IInviteService inviteService, IInviteRepository inviteRepository,
        IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _vendorRepository = vendorRepository;
        _operatorRepository = operatorRepository;
        _vendorUserRepository = vendorUserRepository;
        _userPrincipalService = userPrincipalService;
        _emailService = emailService;
        _inviteService = inviteService;
        _inviteRepository = inviteRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    private bool ValidateString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        return true;
    }

    private void MapVendorToUpdate(Vendor vendor, UpdateVendorDto vendorData)
    {
        vendor.BusinessName = vendorData.BusinessName ?? vendor.BusinessName;
        vendor.Address = vendorData.Address ?? vendor.Address;
        vendor.Email = vendorData.Email ?? vendor.Email;
    }

    public async Task<Response<List<BusinessFrontEndDto>>> SearchOperatorsAsync(OperatorSearchDto operatorSearchDto, CancellationToken cancellationToken)
    {
        var operatorIsValid = ValidateString(operatorSearchDto.Name);
        if (!operatorIsValid)
        {
            return new Response<List<BusinessFrontEndDto>>
            {
                ErrorMessage = "Invalid Operator Name",
                ErrorCode = (int)ErrorCodes.InvalidBusinessName
            };
        }

        var operators = await _operatorRepository.GetOperatorsByNameAsync(operatorSearchDto.Name, cancellationToken);
        var result = operators.Select(o => o.ToFrontEndDto()).ToList();

        return new Response<List<BusinessFrontEndDto>>
        {
            Data = result,
        };
    }

    public async Task<Response<BusinessFrontEndDto>> GetVendorByIdAsync(int vendorId, CancellationToken cancellationToken)
    {
        var vendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);
        var result = vendor.ToFrontEndDto();

        return new Response<BusinessFrontEndDto>
        {
            Data = result,
        };
    }

    public async Task<Response<BusinessFrontEndDto>> UpdateVendorAsync(UpdateVendorDto vendorData, CancellationToken cancellationToken)
    {
        var vendorId = _userPrincipalService.BusinessId!.Value;
        var existingVendor = await _vendorRepository.GetByIdAsync(vendorId, cancellationToken);

        MapVendorToUpdate(existingVendor, vendorData);

        await _vendorRepository.UpdateAsync(existingVendor, cancellationToken);

        var result = existingVendor.ToFrontEndDto();
        return new Response<BusinessFrontEndDto>
        {
            Data = result,
        };
    }

    public async Task<Response<MailMsg>> InviteVendorUserAsync(EmailForInviteDto emailDto, CancellationToken cancellationToken)
    {
        var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var vendorUserId = _userPrincipalService.UserId!.Value;
            var vendorId = _userPrincipalService.BusinessId;
            var vendorUser = await _vendorUserRepository.GetByIdAsync(vendorUserId, cancellationToken);

            var email = emailDto.Email;

            var newVendorUser = new VendorUser { Email = email, VendorId = vendorId, UserType = UserType.VendorUser };

            await _vendorUserRepository.CreateAsync(newVendorUser, cancellationToken);

            var invite = _inviteService.CreateInvite(newVendorUser, vendorUser);
            var inviteUrl = _emailService.CreateInviteUrl(invite.Id);
            await _inviteRepository.CreateAsync(invite, cancellationToken);

            var emailBody = _emailService.GenerateEmailTemplate(email, newVendorUser, inviteUrl);

            var mailMessage = _emailService.CreateMessage(emailBody, vendorUser.Email);

            await _emailService.SendInvitationEmailAsync(mailMessage);

            await transaction.CommitAsync(cancellationToken);

            return new Response<MailMsg>
            {
                Data = mailMessage,
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            
            return new Response<MailMsg>
            {
                ErrorCode = (int)ErrorCodes.InvalidInvitationData,
                ErrorMessage = "Failed to craete an invite in"
            };
        }
    }
}
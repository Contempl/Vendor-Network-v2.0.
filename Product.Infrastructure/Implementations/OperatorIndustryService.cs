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

public class OperatorIndustryService : IOperatorIndustryService
{
    private readonly IOperatorIndustryRepository _operatorIndustryRepository;
    private readonly IUserPrincipalService _userPrincipalService;
    private readonly IOperatorRepository _operatorRepository;
    private readonly ILogger<OperatorIndustryService> _logger;

    public OperatorIndustryService(
        IOperatorIndustryRepository operatorIndustryRepository,
        IUserPrincipalService userPrincipalService,
        IOperatorRepository operatorRepository,
        ILogger<OperatorIndustryService> logger)
    {
        _operatorIndustryRepository = operatorIndustryRepository;
        _userPrincipalService = userPrincipalService;
        _operatorRepository = operatorRepository;
        _logger = logger;
    }

    public async Task<OneOf<int, Error>> RemoveOperatorIndustryAsync(int industryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var operatorId = _userPrincipalService.BusinessId!.Value;
            var operatorIndustry =
                await _operatorIndustryRepository.GetByIdAsync(operatorId, industryId, cancellationToken);

            await _operatorIndustryRepository.DeleteAsync(operatorIndustry, cancellationToken);

            return industryId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error on removing operator industry");
            return new Error();
        }
    }

    public async Task<OneOf<List<OpIndustryFrontEndDto>, Error>> GetOperatorsIndustriesAsync(
        CancellationToken cancellationToken = default)
    {
        var operatorId = _userPrincipalService.BusinessId!.Value;
        var industries = await _operatorIndustryRepository
            .GetOperatorsIndustriesAsync(operatorId, cancellationToken);

        if (!industries.Any())
        {
            _logger.LogWarning("Operator Industries couldn't be fetched");
            return new Error();
        }

        var result = industries.Select(oi => oi.ToFrontEndDto()).ToList();
        return result;
    }


    public async Task<OneOf<OpIndustryFrontEndDto, Error>> CreateOperatorIndustryAsync
        (OperatorIndustryCreationDto industryCreationData, CancellationToken cancellationToken = default)
    {
        try
        {
            var operatorId = _userPrincipalService.BusinessId!.Value;
            var existingOperator = await _operatorRepository.GetByIdAsync(operatorId, cancellationToken);

            var newIndustry = industryCreationData.MapIndustryToCreateOperator(existingOperator);

            await _operatorIndustryRepository.CreateAsync(newIndustry, cancellationToken);

            var result = newIndustry.ToFrontEndDto();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Couldn't create operator industry");
            return new Error();
        }
    }

    public async Task<OneOf<OpIndustryFrontEndDto, Error>> GetOpIndustryByIdAsync(int industryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var operatorId = _userPrincipalService.BusinessId!.Value;
            var industry = await _operatorIndustryRepository.GetByIdAsync(operatorId, industryId, cancellationToken);

            var result = industry.ToFrontEndDto();
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while getting operator industry");
            return new Error();
        }
    }

    public async Task<OneOf<OpIndustryFrontEndDto, NotFoundError, Error>> UpdateOperatorIndustryAsync(int industryId,
        UpdateOperatorIndustryDto industryData, CancellationToken cancellationToken = default)
    {
        try
        {
            var operatorId = _userPrincipalService.BusinessId!.Value;
            var existingIndustry =
                await _operatorIndustryRepository.GetByIdAsync(operatorId, industryId, cancellationToken);

            if (existingIndustry == null)
            {
                _logger.LogWarning("Operator Industry Not Found");
                return new NotFoundError();
            }

            existingIndustry.MapIndustryToUpdate(industryData);
            await _operatorIndustryRepository.UpdateAsync(existingIndustry, cancellationToken);

            var result = existingIndustry.ToFrontEndDto();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occured while updating operator industry");
            return new Error();
        }
    }
}
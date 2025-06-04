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

    public OperatorIndustryService(IOperatorIndustryRepository operatorIndustryRepository, IUserPrincipalService userPrincipalService, IOperatorRepository operatorRepository)
    {
	    _operatorIndustryRepository = operatorIndustryRepository;
	    _userPrincipalService = userPrincipalService;
	    _operatorRepository = operatorRepository;
    }

    public Task CreateAsync(OperatorIndustry operatorIndustry) => _operatorIndustryRepository
        .CreateAsync(operatorIndustry);

    public async Task<Response<int>> RemoveOperatorIndustryAsync(int industryId)
    {
	    var operatorId = _userPrincipalService.BusinessId!.Value;
	    var operatorIndustry = await _operatorIndustryRepository.GetByIdAsync(operatorId, industryId);

	    await _operatorIndustryRepository.DeleteAsync(operatorIndustry);

	    return new Response<int>
	    {
		    Data = industryId,
	    };
    }
    public async Task<Response<List<OpIndustryFrontEndDto>>> GetOperatorsIndustriesAsync()
    {
	    var operatorId = _userPrincipalService.BusinessId!.Value;
	    var industries = await _operatorIndustryRepository.GetOperatorsIndustriesAsync(operatorId);

	    if (!industries.Any())
	    {
		    return new Response<List<OpIndustryFrontEndDto>>
		    {
			    ErrorMessage = "Operator Industries Couldn't be fetched",
			    ErrorCode = (int)ErrorCodes.InvalidOperatorIndustryData,
		    };
	    }
	    
	    var result = industries.Select(oi => oi.ToFrontEndDto()).ToList();
        return new Response<List<OpIndustryFrontEndDto>>
        {
	        Data = result,
        };
    }

    private OperatorIndustry MapIndustryToCreateOperator(Operator @operator, OperatorIndustryCreationDto industryData)
    {
		var newIndustry = new OperatorIndustry
		{
			Name = industryData.Name,
			Address = industryData.Address,
			Latitude = industryData.Latitude,
			Longitude = industryData.Longitude,
			Operator = @operator
		};
        return newIndustry;
	}

    private void MapIndustryToUpdate(OperatorIndustry industry, 
        UpdateOperatorIndustryDto industryData)
    {
		industry.Name = industryData.Name ?? industry.Name;
		industry.Address = industryData.Address ?? industry.Address;
		industry.Latitude = industryData.Latitude ?? industry.Latitude;
		industry.Longitude = industryData.Longitude ?? industry.Longitude;
	}

    public async Task<Response<OpIndustryFrontEndDto>> CreateOperatorIndustryAsync(OperatorIndustryCreationDto industryCreationData)
    {
	    var operatorId = _userPrincipalService.BusinessId!.Value;
	    var existingOperator = await _operatorRepository.GetByIdAsync(operatorId);

	    if (existingOperator == null)
	    {
		    return new Response<OpIndustryFrontEndDto>
		    {
			    ErrorMessage = "Operator User Not Found",
			    ErrorCode = (int)ErrorCodes.UserNotFound,
		    };
	    }

	    var newIndustry = MapIndustryToCreateOperator(existingOperator, industryCreationData);

	    await _operatorIndustryRepository.CreateAsync(newIndustry);

	    var result = newIndustry.ToFrontEndDto();
	    
	    return new Response<OpIndustryFrontEndDto>
	    {
		    Data = result,
	    };
    }

    public async Task<Response<OpIndustryFrontEndDto>> GetOpIndustryByIdAsync(int industryId)
    {
	    var operatorId = _userPrincipalService.BusinessId!.Value;
	    var industry = await _operatorIndustryRepository.GetByIdAsync(operatorId, industryId);

	    return new Response<OpIndustryFrontEndDto>
	    {
		    Data = industry.ToFrontEndDto(),
	    };
    }

    public async Task<Response<OpIndustryFrontEndDto>> UpdateOperatorIndustryAsync(int industryId, UpdateOperatorIndustryDto industryData)
    {
	    var operatorId = _userPrincipalService.BusinessId!.Value;
	    var existingIndustry = await _operatorIndustryRepository.GetByIdAsync(operatorId, industryId);
	    
	    if (existingIndustry == null)
	    {
		    return new Response<OpIndustryFrontEndDto>
		    {
			    ErrorMessage = "Operator Industry Not Found",
			    ErrorCode = (int)ErrorCodes.OperatorIndustryNotFound,
		    };
	    }

	    MapIndustryToUpdate(existingIndustry, industryData);
	    await _operatorIndustryRepository.UpdateAsync(existingIndustry);
	    
	    var result = existingIndustry.ToFrontEndDto();

	    return new Response<OpIndustryFrontEndDto>
	    {
		    Data = result,
	    };
    }
}

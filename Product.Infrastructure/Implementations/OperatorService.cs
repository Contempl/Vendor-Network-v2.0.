using Microsoft.Data.SqlClient;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Implementations;

public class OperatorService : IOperatorService
{
    private readonly IOperatorRepository _operatorRepository;
    private readonly IVendorRepository _vendorRepository;
    private readonly IOperatorIndustryRepository _operatorFacilityRepository;

	public OperatorService(IOperatorRepository operatorRepository, 
        IVendorRepository vendorRepository, IOperatorIndustryRepository operatorFacilityRepository)
	{
		_operatorRepository = operatorRepository;
        _vendorRepository = vendorRepository;
		_operatorFacilityRepository = operatorFacilityRepository;
	}

	public Task CreateAsync(Operator @operator) => _operatorRepository.CreateAsync(@operator);
    public Task DeleteAsync(Operator @operator) => _operatorRepository.DeleteAsync(@operator);
    public Task<Operator?> GetByIdOrDefaultAsync(int id) => _operatorRepository.GetByIdOrDefaultAsync(id);
	public async Task<Operator> GetByIdAsync(int operatorId) => await _operatorRepository.GetByIdAsync(operatorId); //if entity in cache
    public async Task UpdateAsync(Operator oper) => await _operatorRepository.UpdateAsync(oper);
	public List<OperatorIndustry> GetAllOperatorIndustries(List<int> facilityIds)
    {
        if (facilityIds.Count == 0)
        {
            throw new ArgumentException("No facilities provided for search");
        }
        var facilities = _operatorFacilityRepository.GetAll()
            .Where(of => facilityIds.Contains(of.Id)).ToList();

        return facilities;
	}
	public Task<PagedResult<Vendor>> GetVendorsQuery(string searchName, SortOrder sortOrder,
        int pageSize, int pageNumber) => _vendorRepository.GetVendorsQuery(searchName,
            sortOrder, pageSize, pageNumber);
    public void ValidateStringInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new ArgumentNullException($"The field '{nameof(input)}' should't be empty ");
        }
    }
    public Operator MapOperatorFromDto(OperatorRegistrationDto operatorRegistrationData, 
        OperatorUser user) => new Operator
    {
		BusinessName = operatorRegistrationData.BusinessName,
		Adress = operatorRegistrationData.Adress,
		Email = operatorRegistrationData.Email,
		LogoUrl = operatorRegistrationData.LogoUrl,
		Occupation = operatorRegistrationData.Occupation,
		OperatorUsers = new List<OperatorUser> { user }
	};
    public void MapOperatorFromDto(Operator @operator, UpdateOperatorDto operatorData)
    {
		@operator.BusinessName = operatorData.BusinessName ?? @operator.BusinessName;
		@operator.Adress = operatorData.Address ?? @operator.Adress;
		@operator.Email = operatorData.Email ?? @operator.Email;
		@operator.LogoUrl = operatorData.LogoUrl ?? @operator.LogoUrl;
		@operator.Occupation = operatorData.Occupation ?? @operator.Occupation;
	}
	public async Task<List<Vendor>> SearchVendorsAsync(string serviceType, List<OperatorIndustry> operatorFacilities)
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

}   

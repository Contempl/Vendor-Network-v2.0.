using Microsoft.Data.SqlClient;
using Product.Application.Dto;
using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IVendorRepository : IRepository<Vendor>
{
	Task<Vendor> GetByIdAsync(int id, CancellationToken cancellationToken);
	Task<List<Vendor>> GetVendorsWithService(string serviceType, CancellationToken cancellationToken);
	Task<PagedResult<Vendor>> GetVendorsQuery(string searchName, SortOrder sortOrder,
	int pageSize, int pageNumber, CancellationToken cancellationToken);
}
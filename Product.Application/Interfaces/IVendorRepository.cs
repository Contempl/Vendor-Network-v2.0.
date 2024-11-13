using Microsoft.Data.SqlClient;
using Product.Domain.Entity;
using Product.Application.Pagination;
using Product.Infrastructure.Dto;

namespace Product.Application.Interfaces;

public interface IVendorRepository : IRepository<Vendor>
{
	Task<Vendor> GetByIdAsync(int id);
	Task<List<Vendor>> GetVendorsWithService(string serviceType);
	Task<PagedResult<Vendor>> GetVendorsQuery(string searchName, SortOrder sortOrder,
	int pageSize, int pageNumber);
}
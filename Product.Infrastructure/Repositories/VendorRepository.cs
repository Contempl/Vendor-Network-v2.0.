using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Product.Application.Interfaces;
using Product.Domain.Entity;
using Product.Infrastructure.Dto;

namespace Product.Infrastructure.Repositories;

public class VendorRepository : IVendorRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<Vendor> _vendors;

	public VendorRepository(AppDbContext context)
	{
		_context = context;
		_vendors = _context.Vendors;
	}

	public async Task CreateAsync(Vendor entity)
	{
		await _vendors.AddAsync(entity);
		await SaveAsync();
	}
	public async Task DeleteAsync(Vendor vendor)
	{
		_vendors.Remove(vendor);
		await SaveAsync();
	}
	public IQueryable<Vendor> GetAll() => _vendors;
	public async Task<Vendor?> GetByIdOrDefaultAsync(int id) => await _vendors.SingleOrDefaultAsync(v => v.Id == id);
	public async Task<Vendor> GetByIdAsync(int id)
	{
		// Try to get from cache (Redis). IDistributedCache injects in repository. Make gets check cache first. If exists => get from cache. Otherwise get from DB put into cache and give user
		return await _vendors.SingleAsync(v => v.Id == id);
	}

	public async Task UpdateAsync(Vendor vendor)
	{
		// Make update, delete, create update cache. So that entity would be updated not only in db, but also in cache.
		_vendors.Update(vendor);
		await SaveAsync();
	}
	public async Task SaveAsync() => await _context.SaveChangesAsync();
	public async Task<List<Vendor>> GetVendorsWithService(string serviceType)
	{
		return await GetAll()
			.Include(v => v.VendorFacilities)
			.ThenInclude(vf => vf.Services)
			.Where(v => v.VendorFacilities.Any(vf => vf.Services.Any(s => s.Name == serviceType)))
			.ToListAsync();
	}
	public async Task<PagedResult<Vendor>> GetVendorsQuery(string searchName, SortOrder sortOrder,
		int pageSize, int pageNumber)
	{
		var query = _vendors.AsQueryable();

		query = query.Where(v => v.BusinessName.Contains(searchName));

		query = sortOrder == SortOrder.Ascending || sortOrder == SortOrder.Unspecified
			? query.OrderBy(v => v.BusinessName)
			: query.OrderByDescending(v => v.BusinessName);

		var totalCount = await query.CountAsync();
		var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

		return new PagedResult<Vendor>()
		{
			Items = items,
			TotalCount = totalCount
		};
	}
}

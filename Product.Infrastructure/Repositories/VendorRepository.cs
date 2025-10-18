using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Product.Application.Dto;
using Product.Application.Interfaces;
using Product.Application.ServiceInterfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class VendorRepository : IVendorRepository
{
	private readonly AppDbContext _context;
	private readonly DbSet<Vendor> _vendors;
	private readonly IRedisCacheService _reddisCacheService;
	private const string CachePrefix = "Business_";

	public VendorRepository(AppDbContext context, IRedisCacheService reddisCacheService)
	{
		_context = context;
		_reddisCacheService = reddisCacheService;
		_vendors = _context.Vendors;
	}

	public async Task CreateAsync(Vendor entity, CancellationToken cancellationToken = default)
	{
		await _vendors.AddAsync(entity, cancellationToken);
		await SaveAsync(cancellationToken);
		await _reddisCacheService.SetAsync(CachePrefix + entity.Id, entity);
	}
	public Task DeleteAsync(Vendor vendor, CancellationToken cancellationToken = default)
	{
		_vendors.Remove(vendor);
		return SaveAsync(cancellationToken);
	}
	public IQueryable<Vendor> GetAll() => _vendors;

	public async Task<Vendor?> GetByIdOrDefaultAsync(int businessId)
	{
		var cachedBusiness =  await _reddisCacheService.GetAsync<Vendor>(CachePrefix + businessId);
		if (cachedBusiness != null)
		{
			return cachedBusiness;
		}
		var business = await _vendors.SingleOrDefaultAsync(v => v.Id == businessId);
		
		await _reddisCacheService.SetAsync(CachePrefix + businessId, cachedBusiness);
		return business;
	}

	public async Task<Vendor> GetByIdAsync(int businessId, CancellationToken cancellationToken = default)
	{
		var cacheKey = CachePrefix + businessId;
		
		var cachedBusiness = await _reddisCacheService.GetAsync<Vendor>(cacheKey);
		if (cachedBusiness != null)
		{
			return cachedBusiness;
		}
		var business = await _context.Vendors.SingleAsync(v => v.Id == businessId, cancellationToken);
		
		await _reddisCacheService.SetAsync(cacheKey, business);
		return business;
	}

	public Task UpdateAsync(Vendor vendor, CancellationToken cancellationToken = default)
	{
		_reddisCacheService.RemoveAsync(CachePrefix + vendor.Id);
		
		_vendors.Update(vendor);
		
		_reddisCacheService.SetAsync(CachePrefix + vendor.Id, vendor);
		
		return SaveAsync(cancellationToken);
	}

	public Task<List<Vendor>> GetVendorsWithService(string serviceType, CancellationToken cancellationToken = default)
	{
		return GetAll()
			.Include(v => v.VendorFacilities)
			.ThenInclude(vf => vf.Services)
			.Where(v => v.VendorFacilities.Any(vf => vf.Services.Any(s => s.Name == serviceType)))
			.ToListAsync(cancellationToken: cancellationToken);
	}
	public async Task<PagedResult<Vendor>> GetVendorsQuery(string searchName, SortOrder sortOrder,
		int pageSize, int pageNumber, CancellationToken cancellationToken = default)
	{
		var query = _vendors.AsQueryable();

		query = query.Where(v => v.BusinessName.Contains(searchName));

		query = sortOrder == SortOrder.Ascending || sortOrder == SortOrder.Unspecified
			? query.OrderBy(v => v.BusinessName)
			: query.OrderByDescending(v => v.BusinessName);

		var totalCount = query.CountAsync(cancellationToken: cancellationToken).Result;
		var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

		return new PagedResult<Vendor>()
		{
			Items = items,
			TotalCount = totalCount
		};
	}
	private Task SaveAsync(CancellationToken cancellationToken = default) => 
		_context.SaveChangesAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore.Storage;
using Product.Application.Interfaces;
using Product.Domain.Entity;

namespace Product.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context, IRepository<OperatorUser> operatorUsers,
        IRepository<VendorUser> vendorUsers, IRepository<Invite> invites, IRepository<Vendor> vendors, IRepository<Operator> operators)
    {
        _context = context;
        OperatorUsers = operatorUsers;
        VendorUsers = vendorUsers;
        Invites = invites;
        Vendors = vendors;
        Operators = operators;
    }


    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }

    public IRepository<OperatorUser> OperatorUsers { get; set; }
    
    public IRepository<VendorUser> VendorUsers { get; set; }
    
    public IRepository<Invite> Invites { get; set; }
    
    public IRepository<Vendor> Vendors { get; set; }
    
    public IRepository<Operator> Operators { get; set; }

    public void Dispose()
    {
        _context.Dispose();
    }
}
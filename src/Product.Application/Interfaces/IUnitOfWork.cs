using Microsoft.EntityFrameworkCore.Storage;
using Product.Domain.Entity;

namespace Product.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    Task<IDbContextTransaction> BeginTransactionAsync();
    
    public IRepository<OperatorUser> OperatorUsers { get; set; }
    
    public IRepository<VendorUser> VendorUsers { get; set; }
    
    public IRepository<Invite> Invites { get; set; }
    
    public IRepository<Vendor> Vendors { get; set; }
    
    public IRepository<Operator> Operators { get; set; }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Product.Application.ServiceInterfaces;
using Product.Domain.Common;

namespace Product.Infrastructure.Interceptors;

public class DateInterceptor : SaveChangesInterceptor
{
    private readonly IUserPrincipalService _userPrincipalService;

    public DateInterceptor(IUserPrincipalService userPrincipalService)
    {
        _userPrincipalService = userPrincipalService;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = new())
    {
        var dbContext = eventData.Context;
        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
        
        var entries = dbContext.ChangeTracker.Entries<IAuditable>()
            .Where(x => x.State == EntityState.Added || x.State == EntityState.Modified)
            .ToList();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                entry.Property("CreatedBy").CurrentValue = _userPrincipalService.UserId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                entry.Property("UpdatedBy").CurrentValue = _userPrincipalService.UserId;
            }
        }
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
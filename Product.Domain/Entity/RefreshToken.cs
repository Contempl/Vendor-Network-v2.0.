using Product.Domain.Common;

namespace Product.Domain.Entity;

public class RefreshToken : IEntityId<long>
{
    public long Id { get; set; }

    public string Token { get; set; }

    public int UserId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public User User { get; set; }

    public bool Revoked { get; set; }
}
using System.Security.Cryptography;
using System.Text;
using Product.Infrastructure.Implementations.Account;
using Xunit;

namespace Product.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void NewHashesUseDistinctSaltsAndValidateOnlyTheCorrectPassword()
    {
        var first = _hasher.HashThePassword("example password");
        var second = _hasher.HashThePassword("example password");

        Assert.NotEqual(first, second);
        Assert.True(_hasher.ValidatePassword("example password", first));
        Assert.True(_hasher.ValidatePassword("example password", second));
        Assert.False(_hasher.ValidatePassword("wrong password", first));
        Assert.False(_hasher.NeedsRehash(first));
    }

    [Fact]
    public void LegacySha512HashCanBeVerifiedAndRequiresUpgrade()
    {
        var legacy = SHA512.HashData(Encoding.UTF8.GetBytes("example password"));

        Assert.True(_hasher.ValidatePassword("example password", legacy));
        Assert.False(_hasher.ValidatePassword("wrong password", legacy));
        Assert.True(_hasher.NeedsRehash(legacy));
    }

    [Fact]
    public void MalformedHashIsRejected()
    {
        Assert.False(_hasher.ValidatePassword("example password", Array.Empty<byte>()));
        Assert.False(_hasher.ValidatePassword("example password", new byte[57]));
        Assert.False(_hasher.NeedsRehash(new byte[57]));
    }
}

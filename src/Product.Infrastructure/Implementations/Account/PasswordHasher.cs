using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Product.Application.ServiceInterfaces;

namespace Product.Infrastructure.Implementations.Account;

public class PasswordHasher : IPasswordHasher
{
    private static readonly byte[] Magic = "VNPW"u8.ToArray();
    private const byte Version = 1;
    private const int Iterations = 600_000;
    private const int SaltLength = 16;
    private const int HashLength = 32;
    private const int HeaderLength = 9;
    private const int StoredLength = HeaderLength + SaltLength + HashLength;

    public byte[] HashThePassword(string password)
    {
        var stored = new byte[StoredLength];
        Magic.CopyTo(stored, 0);
        stored[4] = Version;
        BinaryPrimitives.WriteInt32BigEndian(stored.AsSpan(5, 4), Iterations);
        RandomNumberGenerator.Fill(stored.AsSpan(HeaderLength, SaltLength));

        Rfc2898DeriveBytes.Pbkdf2(password,
            stored.AsSpan(HeaderLength, SaltLength),
            stored.AsSpan(HeaderLength + SaltLength, HashLength),
            Iterations, HashAlgorithmName.SHA256);
        return stored;
    }

    public bool ValidatePassword(string enteredPassword, byte[] storedHashedPassword)
    {
        if (storedHashedPassword is null)
            return false;

        // Old accounts stored a raw SHA-512 digest. Upgrade them after a successful login.
        if (storedHashedPassword.Length == 64)
        {
            var legacyHash = SHA512.HashData(Encoding.UTF8.GetBytes(enteredPassword));
            return CryptographicOperations.FixedTimeEquals(legacyHash, storedHashedPassword);
        }

        if (!TryReadCurrentHash(storedHashedPassword, out var iterations))
            return false;

        var actualHash = Rfc2898DeriveBytes.Pbkdf2(enteredPassword,
            storedHashedPassword.AsSpan(HeaderLength, SaltLength), iterations,
            HashAlgorithmName.SHA256, HashLength);
        return CryptographicOperations.FixedTimeEquals(actualHash,
            storedHashedPassword.AsSpan(HeaderLength + SaltLength, HashLength));
    }

    public bool NeedsRehash(byte[] storedHashedPassword) =>
        storedHashedPassword is not null &&
        (storedHashedPassword.Length == 64 ||
         (TryReadCurrentHash(storedHashedPassword, out var iterations) && iterations < Iterations));

    private static bool TryReadCurrentHash(byte[] stored, out int iterations)
    {
        iterations = 0;
        if (stored.Length != StoredLength ||
            !stored.AsSpan(0, Magic.Length).SequenceEqual(Magic) || stored[4] != Version)
            return false;

        iterations = BinaryPrimitives.ReadInt32BigEndian(stored.AsSpan(5, 4));
        return iterations > 0 && iterations <= Iterations;
    }
}

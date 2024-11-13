using System.Security.Cryptography;
using System.Text;
using Product.Application.ServiceInterfaces;

namespace Product.Infrastructure.Implementations.Account;

public class PasswordHasher : IPasswordHasher
{
	public byte[] HashThePassword(string password)
	{
		using (var sha = SHA512.Create())
		{
			var passwordHash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
			return passwordHash;
		}
	}
	public bool ValidatePassword(string enteredPassword, byte[] storedHashedPassword)
	{
		using (var sha = SHA512.Create())
		{
			var utf8 = Encoding.UTF8.GetBytes(enteredPassword);
			using var stream = new MemoryStream(utf8);
			byte[] enteredPasswordBytes = sha.ComputeHash(stream);

			for (int i = 0; i < storedHashedPassword.Length; i++)
			{
				if (storedHashedPassword[i] != enteredPasswordBytes[i])
				{
					return false;
				}
			}
			return true;
		}
	}
}

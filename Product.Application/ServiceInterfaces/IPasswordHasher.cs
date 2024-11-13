namespace Product.Application.ServiceInterfaces;

public interface IPasswordHasher
{
	byte[] HashThePassword(string password);
	public bool ValidatePassword(string enteredPassword, byte[] storedHashedPassword);
}

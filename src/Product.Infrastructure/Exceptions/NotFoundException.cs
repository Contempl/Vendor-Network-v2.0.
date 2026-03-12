namespace Product.Infrastructure.Exceptions;

public class NotFoundException<TEntity> : NotFoundException
{
	public NotFoundException() : base($"Entity of type {typeof(TEntity).Name} not found.") { }
	public NotFoundException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
	public NotFoundException() { }
	public NotFoundException(string message) : base(message) { }
}
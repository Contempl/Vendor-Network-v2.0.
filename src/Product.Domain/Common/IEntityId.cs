namespace Product.Domain.Common;

public interface IEntityId<T> where T : struct
{
    T Id { get; set; }
}
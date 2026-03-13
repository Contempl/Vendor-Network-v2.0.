namespace Product.Domain.Result;

public struct NotFoundError
{
    public string? Name { get;  private set; }

    public NotFoundError(string? name)
    {
        Name = name;
    }
}
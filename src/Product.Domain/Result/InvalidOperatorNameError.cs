namespace Product.Domain.Result;

public struct InvalidOperatorNameError
{
    public string? Name { get;  private set; }

    public InvalidOperatorNameError(string? name)
    {
        Name = name;
    }
}
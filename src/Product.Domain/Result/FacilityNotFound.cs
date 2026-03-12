namespace Product.Domain.Result;

public struct FacilityNotFound
{
    public string? Name { get;  private set; }

    public FacilityNotFound(string? name)
    {
        Name = name;
    }
}
namespace Product.Domain.Enum;

// Keep the existing API numeric values independent of the database provider.
public enum SortOrder
{
    Unspecified = -1,
    Ascending = 0,
    Descending = 1
}

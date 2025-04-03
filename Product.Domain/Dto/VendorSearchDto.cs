using Microsoft.Data.SqlClient;

namespace Product.Domain.Dto;

public class VendorSearchDto
{
    public string VendorName { get; set; }

    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

    public int PageSize { get; set; } = 10;

    public int PageNumber { get; set; } = 1;
}
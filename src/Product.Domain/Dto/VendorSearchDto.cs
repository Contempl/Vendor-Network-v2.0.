using System.ComponentModel.DataAnnotations;
using Product.Domain.Enum;

namespace Product.Domain.Dto;

public class VendorSearchDto
{
    [Required]
    public string VendorName { get; set; }

    public SortOrder SortOrder { get; set; } = SortOrder.Ascending;

    public int PageSize { get; set; } = 10;

    public int PageNumber { get; set; } = 1;
}
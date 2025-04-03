namespace Product.Domain.Dto;

public class SearchVendorsForIndustriesDto
{
    public string ServiceType { get; set; }

    public List<int> IndustriesLocationIds { get; set; }
}
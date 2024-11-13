namespace Product.WebApi.Pagination;

public class PagedList<T>
{
    public PagedList(List<T> items, int pageSize, int pageNumber, int totalCount)
	{
		Items = items;
		PageSize = pageSize;
		PageNumber = pageNumber;
		TotalCount = totalCount;
	}

	public List<T> Items { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasNextPage => PageNumber * PageSize < TotalCount;
    public bool HasPreviousPage => PageNumber > 1;

}

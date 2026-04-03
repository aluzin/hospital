namespace Hospital.Api.Contracts;

public class PagedResponse<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

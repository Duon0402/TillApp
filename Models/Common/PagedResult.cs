namespace TillApp.Models
{
    public record PagedResult<T>(List<T> Items, int TotalCount, int PageNumber, int PageSize)
    {
        public int TotalPage => (int)Math.Ceiling(TotalCount / (double)TotalPage);
    }
}

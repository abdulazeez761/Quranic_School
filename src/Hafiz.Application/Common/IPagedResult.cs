namespace Hafiz.Application.Common
{
    public interface IPagedResult
    {
        int PageNumber { get; set; }
        int PageSize { get; set; }
        int TotalCount { get; set; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
        int StartRecord { get; }
        int EndRecord { get; }
    }
}

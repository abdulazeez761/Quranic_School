using System;
using System.Collections.Generic;

namespace Hafiz.Application.Common
{
    public class PagedResult<T> : IPagedResult
    {
        public List<T> Items { get; set; } = new();
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalCount { get; set; } = 0;

        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
        public bool HasPreviousPage => PageNumber > 1 && TotalPages > 0;
        public bool HasNextPage => PageNumber < TotalPages;
        public int StartRecord
        {
            get
            {
                if (TotalCount == 0) return 0;
                int start = ((PageNumber - 1) * PageSize) + 1;
                return Math.Min(start, TotalCount);
            }
        }
        public int EndRecord => Math.Min(PageNumber * PageSize, TotalCount);

        public PagedResult() { }

        public PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
        {
            Items = items ?? new List<T>();
            TotalCount = Math.Max(0, totalCount);
            PageSize = pageSize < 1 ? 10 : pageSize;

            int totalPages = PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;
            if (pageNumber < 1)
            {
                PageNumber = 1;
            }
            else if (totalPages > 0 && pageNumber > totalPages)
            {
                PageNumber = totalPages;
            }
            else
            {
                PageNumber = pageNumber;
            }
        }
    }
}

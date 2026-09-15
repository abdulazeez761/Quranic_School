using System;
using System.Collections.Generic;
using System.Linq;
using Hafiz.Application.Common;

namespace Hafiz.Application.Extensions
{
    public static class PaginationExtensions
    {
        public static PagedResult<T> ToPagedResult<T>(
            this IEnumerable<T> source,
            int pageNumber,
            int pageSize
        )
        {
            pageSize = pageSize < 1 ? 10 : pageSize;

            var list = source as IList<T> ?? source?.ToList() ?? new List<T>();
            var totalCount = list.Count;
            int totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }
            else if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            var items = totalCount > 0
                ? list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList()
                : new List<T>();

            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        public static PagedResult<T> ToPagedResult<T>(
            this IEnumerable<T> pagedItems,
            int totalCount,
            int pageNumber,
            int pageSize
        )
        {
            totalCount = Math.Max(0, totalCount);
            pageSize = pageSize < 1 ? 10 : pageSize;
            int totalPages = totalCount > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }
            else if (totalPages > 0 && pageNumber > totalPages)
            {
                pageNumber = totalPages;
            }

            var items = pagedItems as List<T> ?? pagedItems?.ToList() ?? new List<T>();
            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }
    }
}

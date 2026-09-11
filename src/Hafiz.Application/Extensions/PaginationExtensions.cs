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
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var list = source as IList<T> ?? source?.ToList() ?? new List<T>();
            var totalCount = list.Count;
            var items = list.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }

        public static PagedResult<T> ToPagedResult<T>(
            this IEnumerable<T> pagedItems,
            int totalCount,
            int pageNumber,
            int pageSize
        )
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var items = pagedItems as List<T> ?? pagedItems?.ToList() ?? new List<T>();
            return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
        }
    }
}

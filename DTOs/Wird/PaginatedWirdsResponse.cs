using System;
using System.Collections.Generic;
using Hifz.Models;

namespace Hifz.DTOs.Wird
{
    public class PaginatedWirdsResponse
    {
        public List<WirdAssignment> Wirds { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }

        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginatedWirdsResponse()
        {
            Wirds = new List<WirdAssignment>();
        }

        public PaginatedWirdsResponse(
            List<WirdAssignment> wirds,
            int pageNumber,
            int pageSize,
            int totalCount,
            int completedCount,
            int pendingCount
        )
        {
            Wirds = wirds;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            CompletedCount = completedCount;
            PendingCount = pendingCount;
        }
    }
}

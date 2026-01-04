using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Pagination
{
    public class PaginationMetadata
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
        public int? PreviousPage { get; set; }
        public int? NextPage { get; set; }

        public PaginationMetadata(int currentPage, int pageSize, int totalCount)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            HasPrevious = currentPage > 1;
            HasNext = currentPage < TotalPages;
            PreviousPage = HasPrevious ? currentPage - 1 : null;
            NextPage = HasNext ? currentPage + 1 : null;
        }
    }
}

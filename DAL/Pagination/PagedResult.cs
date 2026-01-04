using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Pagination
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; }
        public PaginationMetadata Metadata { get; set; }

        public PagedResult(IEnumerable<T> items, PaginationMetadata metadata)
        {
            Items = items;
            Metadata = metadata;
        }
    }
}

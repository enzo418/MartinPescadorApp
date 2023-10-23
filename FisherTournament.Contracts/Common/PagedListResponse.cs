using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FisherTournament.Contracts.Common
{
    public class PagedListResponse<T>
    {
        public List<T> Items { get; set; }

        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public bool HasNextPage => Page * PageSize < TotalCount;

        public bool HasPreviousPage => Page > 1;
    }
}
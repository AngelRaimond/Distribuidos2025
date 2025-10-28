using System.Collections.Generic;

namespace PokedexApi.Shared.Dto
{
    public class PagedResponse<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public IList<T> Data { get; set; } = new List<T>();
    }
}

using NovelVision.Services.Catalog.API.Models.Responses;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NovelVision.Services.Catalog.API.Models.Requests
{
    public class PaginatedResponse<T> : ApiResponse<List<T>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginatedResponse()
        {
            Data = new List<T>();
        }
    }

}

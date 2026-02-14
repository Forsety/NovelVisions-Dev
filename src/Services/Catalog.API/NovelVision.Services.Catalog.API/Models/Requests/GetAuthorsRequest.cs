namespace NovelVision.Services.Catalog.API.Models.Requests
{
    public class GetAuthorsRequest
    {
        public bool? Verified { get; set; }
        public string? SearchTerm { get; set; }
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
    }

}

using NovelVision.Services.Catalog.API.Models.Responses;

namespace NovelVision.Services.Catalog.API.Models.Requests
{
    public class ErrorResponse : ApiResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public int? Status { get; set; }
        public Dictionary<string, List<string>>? ValidationErrors { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Application.DTOs
{
    public record ExternalSourceDto
    {
        public string SourceType { get; init; } = string.Empty;
        public string ExternalId { get; init; } = string.Empty;
        public string? SourceUrl { get; init; }
        public DateTime ImportedAt { get; init; }
        public DateTime? LastSyncedAt { get; init; }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NovelVision.Services.Catalog.Application.DTOs
{
    public record CoverImageDto
    {
        public string? SmallUrl { get; init; }
        public string? MediumUrl { get; init; }
        public string? LargeUrl { get; init; }
        public string? LocalPath { get; init; }
        public int? Width { get; init; }
        public int? Height { get; init; }
        public string Format { get; init; } = string.Empty;
        public bool HasCover { get; init; }
    }

}

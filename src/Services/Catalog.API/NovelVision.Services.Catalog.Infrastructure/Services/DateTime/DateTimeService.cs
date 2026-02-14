using System;
using NovelVision.Services.Catalog.Application.Common.Interfaces;

namespace NovelVision.Services.Catalog.Infrastructure.Services.DateTime;

public class DateTimeService : IDateTime
{
    public System.DateTime Now => System.DateTime.Now;
    public System.DateTime UtcNow => System.DateTime.UtcNow;
}

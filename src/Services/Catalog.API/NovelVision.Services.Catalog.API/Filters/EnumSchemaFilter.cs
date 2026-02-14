using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NovelVision.Services.Catalog.API.Filters;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            var enumValues = Enum.GetValues(context.Type);
            var enumNames = Enum.GetNames(context.Type);

            schema.Enum = enumValues
                .Cast<object>()
                .Select(e => new OpenApiString(e.ToString()))
                .ToList<IOpenApiAny>();

            schema.Description = string.Join(", ", enumNames);
        }
    }
}

namespace ProjetoTech.Filters
{
    using Microsoft.OpenApi.Models;
    using Swashbuckle.AspNetCore.SwaggerGen;
    using System.Collections.Generic;

    public class MetricsEndpointDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Paths.Add("/metrics", new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    [OperationType.Get] = new OpenApiOperation
                    {
                        Tags = new List<OpenApiTag> { new OpenApiTag { Name = "Metrics" } },
                        Summary = "Exposes Prometheus metrics",
                        Responses = new OpenApiResponses
                        {
                            ["200"] = new OpenApiResponse { Description = "Metrics" }
                        }
                    }
                }
            });
        }
    }

}

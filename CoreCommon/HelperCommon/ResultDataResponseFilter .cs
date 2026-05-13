using Microsoft.OpenApi.Models;  // ← was just Microsoft.OpenApi
using Swashbuckle.AspNetCore.SwaggerGen;

public class ResultDataResponseFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var returnType = context.MethodInfo.ReturnType;
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name.Contains("Task"))
        {
            returnType = returnType.GetGenericArguments().First();
        }
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition().Name.Contains("ResultData"))
        {
            var innerType = returnType.GetGenericArguments().FirstOrDefault();
            operation.Responses.TryAdd("200", new OpenApiResponse
            {
                Description = "Success",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(innerType, context.SchemaRepository)
                    }
                }
            });
            operation.Responses.TryAdd("400", new OpenApiResponse { Description = "Bad Request" });
            operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            operation.Responses.TryAdd("404", new OpenApiResponse { Description = "Not Found" });
            operation.Responses.TryAdd("500", new OpenApiResponse { Description = "Internal Server Error" });
        }
    }
}
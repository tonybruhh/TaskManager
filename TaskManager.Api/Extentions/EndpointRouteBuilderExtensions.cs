using TaskManager.Api.Endpoints;

namespace TaskManager.Api.Extentions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        return app;
    }
}
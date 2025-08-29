using TaskManager.Api.Endpoints;

namespace TaskManager.Api.Extensions;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapApi(this IEndpointRouteBuilder app)
    {
        app.MapAuthEndpoints();
        app.MapTaskEndpoints();
        return app;
    }
}
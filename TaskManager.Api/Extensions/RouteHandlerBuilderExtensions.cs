namespace TaskManager.Api.Extensions;

public static class RouteHandlerBuilderExtensions
{
    public static RouteHandlerBuilder ValidateWith<T>(this RouteHandlerBuilder b) where T : class =>
        b.AddEndpointFilter(new ValidationFilter<T>());
    
}
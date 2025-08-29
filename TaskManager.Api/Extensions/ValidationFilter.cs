using System.Security.Cryptography.X509Certificates;
using FluentValidation;

namespace TaskManager.Api.Extensions;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();
        if (validator is null)
            return await next(context);

        var model = context.Arguments.OfType<T>().FirstOrDefault();
        if (model is null)
            return Results.BadRequest("Request body is missing or invalid.");

        var result = await validator.ValidateAsync(model);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return Results.ValidationProblem(errors, statusCode: 400);
        }

        return await next(context);
    }
}
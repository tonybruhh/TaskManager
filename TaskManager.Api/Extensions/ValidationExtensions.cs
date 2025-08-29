using FluentValidation;
using FluentValidation.AspNetCore;
using TaskManager.Api.Contracts;

namespace TaskManager.Api.Extensions;

public static class ValidationExtensions
{
    public static IServiceCollection AddRequestValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();

        services.AddScoped<IValidator<TaskCreateRequest>, TaskCreateValidator>();
        services.AddScoped<IValidator<TaskUpdateRequest>, TaskUpdateValidator>();

        return services;
    }

    private sealed class TaskCreateValidator : AbstractValidator<TaskCreateRequest>
    {
        public TaskCreateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => x.Description is not null);
        }
    }

    private sealed class TaskUpdateValidator : AbstractValidator<TaskUpdateRequest>
    {
        public TaskUpdateValidator()
        {
            RuleFor(x => x.Title)
                .MaximumLength(200)
                .When(x=> x.Title is not null);

            RuleFor(x => x.Description)
                .MaximumLength(2000)
                .When(x => x.Description is not null);
        }
    }
}
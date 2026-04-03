using Hospital.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.Api.Infrastructure;

public static class ValidationErrorResponseFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors.Select(error => new ValidationErrorItemResponse
            {
                Field = ToCamelCasePath(x.Key),
                Message = error.ErrorMessage
            }))
            .ToArray();

        return new BadRequestObjectResult(new ErrorResponse
        {
            Type = "ValidationError",
            Data = new ErrorDataResponse
            {
                Message = "Validation failed.",
                Errors = errors
            }
        });
    }

    private static string ToCamelCasePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        return string.Join(
            ".",
            path.Split('.', StringSplitOptions.RemoveEmptyEntries)
                .Select(ToCamelCase));
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
        {
            return value;
        }

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}

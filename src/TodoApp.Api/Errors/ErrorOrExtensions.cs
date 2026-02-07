using ErrorOr;
using Microsoft.AspNetCore.Mvc;

namespace TodoApp.Api.Errors;

public static class ErrorOrExtensions
{
    public static IActionResult ToActionResult<T>(
        this ErrorOr<T> result,
        ControllerBase controller)
    {
        return result.Match(
            value => controller.Ok(value),
            errors => controller.ProblemFromErrors(errors));
    }

    public static IActionResult ToCreatedResult<T>(
        this ErrorOr<T> result,
        ControllerBase controller,
        string actionName,
        Func<T, object> routeValuesFactory)
    {
        return result.Match(
            value => controller.CreatedAtAction(
                actionName,
                routeValuesFactory(value),
                value),
            errors => controller.ProblemFromErrors(errors));
    }

    public static IActionResult ToNoContentResult(
        this ErrorOr<Deleted> result,
        ControllerBase controller)
    {
        return result.Match(
            _ => controller.NoContent(),
            errors => controller.ProblemFromErrors(errors));
    }

    public static IActionResult ProblemFromErrors(
        this ControllerBase controller,
        List<Error> errors)
    {
        var first = errors[0];

        var status = first.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = first.Code,
            Detail = first.Description
        };

        problem.Extensions["errors"] = errors.Select(e => new
        {
            e.Code,
            e.Description,
            Type = e.Type.ToString()
        });

        return controller.StatusCode(status, problem);
    }
}

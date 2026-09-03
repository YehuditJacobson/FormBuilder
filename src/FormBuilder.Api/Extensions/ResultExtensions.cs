using FormBuilder.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Api.Extensions;

/// <summary>Maps a <see cref="Result{TValue}"/> onto an <see cref="IActionResult"/> — a value on success, a problem response on failure.</summary>
internal static class ResultExtensions
{
    public static IActionResult ToActionResult<TValue>(
        this Result<TValue> result,
        ControllerBase controller,
        Func<TValue, IActionResult> onSuccess)
        => result.IsSuccess ? onSuccess(result.Value) : ToProblem(result.Error, controller);

    private static ObjectResult ToProblem(Error error, ControllerBase controller)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError,
        };

        return controller.Problem(detail: error.Message, statusCode: statusCode, title: error.Type.ToString());
    }
}

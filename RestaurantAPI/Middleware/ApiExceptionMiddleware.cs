using Application.Exceptions;

namespace RestaurantAPI.Middleware;

public class ApiExceptionMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            await WriteProblem(context, StatusCodes.Status404NotFound, ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteProblem(context, StatusCodes.Status409Conflict, ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            await WriteProblem(context, StatusCodes.Status400BadRequest, ex.Message);
        }
    }

    private static Task WriteProblem(HttpContext ctx, int statusCode, string detail)
    {
        ctx.Response.ContentType = "application/problem+json";
        ctx.Response.StatusCode = statusCode;
        var problem = Results.Problem(
            statusCode: statusCode,
            detail: detail,
            title: "Error de negocio"
        );
        return problem.ExecuteAsync(ctx);
    }
}

using Product.Infrastructure.Exceptions;
using Product.Infrastructure.Helper_Classes;

namespace Product.WebApi.Middleware;

public class MyExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<MyExceptionHandlingMiddleware> _logger;

	public MyExceptionHandlingMiddleware(RequestDelegate next, ILogger<MyExceptionHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}

		catch (NotFoundException ex)
		{
			Type exceptionType = ex.GetType();

			if (exceptionType.IsGenericType)
			{
				Type genericType = ex.GetType().GetGenericArguments().Single();

				_logger.LogError(ex, $"Resource not found {nameof(genericType)}.");
			}
			else
			{
				_logger.LogError(ex, "Not found exception.");
			}

			context.Response.StatusCode = StatusCodes.Status404NotFound;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occured");

			context.Response.StatusCode = StatusCodes.Status500InternalServerError;

			var errorResponse = new ErrorResponse("An error occrued", ex.Message );
			await context.Response.WriteAsJsonAsync(errorResponse);
		}
	}
}

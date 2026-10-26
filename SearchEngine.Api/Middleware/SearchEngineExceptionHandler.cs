using Microsoft.AspNetCore.Diagnostics;
using SearchEngine.Core.Exceptions;

namespace SearchEngine.Api.Middleware
{
    public class SearchEngineExceptionHandler : IExceptionHandler
    {
        private readonly IProblemDetailsService _problemDetails;

        public SearchEngineExceptionHandler(IProblemDetailsService problemDetails)
        {
            _problemDetails = problemDetails;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken ct
        )
        {
            var (status, title) = exception switch
            {
                IndexNotFoundException => (StatusCodes.Status404NotFound, "Index not found"),
                InvalidInputException => (StatusCodes.Status400BadRequest, "Invalid request"),
                _ => (0, ""),
            };

            if (status == 0)
                return false;

            context.Response.StatusCode = status;

            return await _problemDetails.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = context,
                    Exception = exception,
                    ProblemDetails =
                    {
                        Status = status,
                        Title = title,
                        Detail = exception.Message,
                    },
                }
            );
        }
    }
}

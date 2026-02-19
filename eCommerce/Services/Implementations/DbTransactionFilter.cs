using ECommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace ECommerce.Services.Implementations
{
    public sealed class DbTransactionFilter : IAsyncActionFilter
    {
        private readonly ECommerceDbContext _dbContext;

        public DbTransactionFilter(ECommerceDbContext dbContext) { _dbContext = dbContext; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Don't start a transaction for GET requests or actions that opt out.
            bool skip = context.HttpContext.Request.Method == HttpMethods.Get
                || context.ActionDescriptor.EndpointMetadata
                    .OfType<SkipDbTransactionAttribute>().Any();

            if (skip)
            {
                await next();
                return;
            }

            await using var tx = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                ActionExecutedContext executedContext = await next();

                // Commit only if no exception was thrown and the result is not an error status.
                // Note: at this stage, IActionResult hasn't been executed yet, so Response.StatusCode
                // is unreliable. We check the result type instead.
                bool isError = executedContext.Exception != null
                    || executedContext.Result is IStatusCodeActionResult { StatusCode: >= 400 };

                if (isError)
                    await tx.RollbackAsync();
                else
                    await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}

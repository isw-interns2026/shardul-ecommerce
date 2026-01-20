using ECommerce.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce
{
    public sealed class DbTransactionFilter : IAsyncActionFilter
    {
        private readonly ECommerceDbContext _dbContext;

        public DbTransactionFilter(ECommerceDbContext dbContext) { this._dbContext = dbContext; }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {   
            // Don't start a transaction for GET requests. 
            if (context.HttpContext.Request.Method == HttpMethods.Get)
            {
                await next();
                return;
            }

            await using var tx = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                ActionExecutedContext executedContext = await next();

                // Commit only if no exceptions are thrown and successful HTTP response
                if (executedContext.Exception == null && context.HttpContext.Response.StatusCode is >= 200 and < 300)
                {
                    await tx.CommitAsync();
                }
                else
                {
                    await tx.RollbackAsync();
                }
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}

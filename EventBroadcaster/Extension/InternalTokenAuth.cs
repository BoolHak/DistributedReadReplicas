using Commun;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EventBroadcaster.Extension
{
    public class InternalTokenAuth : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var header = context.HttpContext.Request.Headers;
            var headerExists = header.TryGetValue(Constants.RBS_TOKEN_NAME, out var token);
            if (!headerExists || token != Constants.RBS_TOKEN_VALUE)
            {
                context.Result = new UnauthorizedResult();
            }
            else
            {
                await next();
            }
        }
    }
}

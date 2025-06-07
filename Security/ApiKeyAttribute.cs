using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class ApiKeyAttribute : Attribute, IAuthorizationFilter
    {
        private const string HeaderName = "x-api-key";

        public void OnAuthorization(AuthorizationFilterContext ctx)
        {
            var cfg = ctx.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var expected = cfg["InternalApiKey"];        

            if (string.IsNullOrEmpty(expected))
            {
                ctx.Result = new StatusCodeResult(500);  
                return;
            }

            if (!ctx.HttpContext.Request.Headers.TryGetValue(HeaderName, out var provided)
                || !String.Equals(provided, expected, StringComparison.Ordinal))
            {
                ctx.Result = new UnauthorizedResult();    
            }
        }
    }
}

using WebApi.Middlewares;

namespace WebApi.Middlewares
{
    public class LastLoginMiddleware
    {
        private readonly RequestDelegate _next;

        public LastLoginMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var cookie = context.Request.Cookies["last-login"];
            if (string.IsNullOrEmpty(cookie))
            {
                context.Response.Cookies.Append("last-login", DateTime.Today.ToShortDateString(), new CookieOptions
                {
                    Expires = DateTime.Today.AddDays(1)
                });
            }

            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }
    }
}

public static class LastLoginMiddlewareExtensions
{
    public static IApplicationBuilder UseLastLogin(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LastLoginMiddleware>();
    }
}
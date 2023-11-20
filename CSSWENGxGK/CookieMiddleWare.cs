using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

public class MyCookieMiddleware
{
    private readonly RequestDelegate _next;

    public MyCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Cookies.TryGetValue("MyCookie", out string cookieValue))
        {
            if (!string.IsNullOrEmpty(cookieValue) && int.TryParse(cookieValue, out int userId))
            {
                context.Session.SetInt32("User_ID", userId);
            }
        }

        await _next(context);
    }
}

public static class MyCookieMiddlewareExtensions
{
    public static IApplicationBuilder UseMyCookieMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MyCookieMiddleware>();
    }
}

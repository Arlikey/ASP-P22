using ASP_P22.Data.Entities;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_P22.Middleware.Auth
{
    public class AuthSessionMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Query.Keys.Contains("logout"))
            {
                context.Session.Remove("authUser");
                context.Items.Remove("authUser");
                context.Response.Redirect(context.Request.Path);
            }
            else if (context.Session.Keys.Contains("authUser"))
            {
                Data.Entities.User user = JsonSerializer.Deserialize<User>(context.Session.GetString("authUser")!)!;

                context.User = new System.Security.Claims.ClaimsPrincipal(
                    new ClaimsIdentity(
                        [
                            new Claim(ClaimTypes.Sid, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Name),
                            new Claim(ClaimTypes.Email, user.Email)
                        ],
                        nameof(AuthSessionMiddleware)
                    )
                );
            }
            await _next(context);
        }
    }

    public static class AuthSessionMiddlewareExtensions
    { 
        public static IApplicationBuilder UseAuthSession(this IApplicationBuilder builder) 
        { 
            return builder.UseMiddleware<AuthSessionMiddleware>(); 
        } 
    }
}

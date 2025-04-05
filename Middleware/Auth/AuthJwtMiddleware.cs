using ASP_P22.Data;
using ASP_P22.Data.Entities;
using ASP_P22.Models;
using ASP_P22.Services.Hash;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_P22.Middleware.Auth
{
	public class AuthJwtMiddleware(RequestDelegate next)
	{
		private readonly RequestDelegate _next = next;

		public async Task InvokeAsync(HttpContext context, DataContext dataContext, IConfiguration configuration, IHashService hashService)
		{
			string authMessage = "";
			string header = "", payload = "", signature = "";
			string authHeader = context.Request.Headers.Authorization.ToString();
			if (string.IsNullOrEmpty(authHeader))
			{
				authMessage = "Missing 'Authorization' Header";
			}
			else
			{
				string authScheme = "Bearer ";
				if (!authHeader.StartsWith(authScheme))
				{
					authMessage = $"Помилка схеми авторизації: потрібна '{authScheme}'.";
				}
				else
				{
					string jwt = authHeader[authScheme.Length..];
					string[] parts = jwt.Split('.');
					if (parts.Length != 3)
					{
						authMessage = $"Invalid JWT Format (splitting error)";
					}
					else
					{
						header = parts[0];
						payload = parts[1];
						signature = parts[2];
					}
				}
			}

			if (authMessage == "")
			{
				string secret = configuration.GetSection("Jwt").GetSection("Secret").Value!;
				string signatureRight = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(hashService.Digest(secret + $"{header}.{payload}")));
				if (signature != signatureRight)
				{
					authMessage = "JWT Signature Error";
				}
				else
				{
					JwtToken jwtToken = JsonSerializer.Deserialize<JwtToken>(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload)))!;
					context.User = new System.Security.Claims.ClaimsPrincipal(
						new ClaimsIdentity(
						[
							new Claim(ClaimTypes.Sid, jwtToken.Sub.ToString()!),
							new Claim(ClaimTypes.Name, jwtToken.Name),
							new Claim(ClaimTypes.Email, jwtToken.Email),
							new Claim(ClaimTypes.NameIdentifier, jwtToken.Slug),
						],
						nameof(AuthJwtMiddleware)
						)
					);
					Console.WriteLine(jwtToken.Name);
				}
			}
			Console.WriteLine(authMessage);
			context.Items.Add(nameof(AuthJwtMiddleware), authMessage);
			await _next(context);
		}
	}
	public static class AuthJwtMiddlewareExtensions
	{
		public static IApplicationBuilder UseJwtToken(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<AuthJwtMiddleware>();
		}
	}
}

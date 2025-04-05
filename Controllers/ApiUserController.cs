using ASP_P22.Data;
using ASP_P22.Data.Entities;
using ASP_P22.Models;
using ASP_P22.Services.Hash;
using ASP_P22.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASP_P22.Controllers
{
	[Route("api/user")]
	[ApiController]
	public class ApiUserController(DataAccessor dataAccessor, IStorageService storageService, IHashService hashService, IConfiguration configuration) : ControllerBase
	{
		private readonly IStorageService _storageService = storageService;
		private readonly DataAccessor _dataAccessor = dataAccessor;
		private readonly IHashService _hashService = hashService;
		private readonly IConfiguration _configuration = configuration;

		[HttpGet]
		public RestResponseModel Authenticate()
		{
			RestResponseModel model = new()
			{
				CacheLifetime = 86400,
				Description = "User API: Authenticate",
				Meta = new()
				{
					{ "locale", "uk" },
					{ "dataType", "object" }
				}
			};
			UserAccess? access = null;
			try
			{
				access = _dataAccessor.BasicAuthenticate();
			}
			catch (Exception ex) 
			{
				model.Status.Code = 500;
				model.Status.Phrase = "Internal Server Error";
				model.Status.isSuccess = false;
				model.Description = ex.Message;
				return model;
			}
			if(access == null)
			{
				model.Status.Code = 401;
				model.Status.Phrase = "Unauthorized";
				model.Status.isSuccess = false;
				model.Description = "Authentication failed";
				return model;
			}
			AuthToken authToken = _dataAccessor.CreateTokenForUserAccess(access);
			model.Data = AuthTokenToJwt(authToken);
			return model;
		}

		private string AuthTokenToJwt(AuthToken authToken)
		{
			string header = "{\"alg\":\"HS256\",\"typ\":\"JWT\"}";
			string header64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(header));
			string payload = JsonSerializer.Serialize(new JwtToken()
			{
				Jti = authToken.Jti,
				Sub = authToken.UserAccess.UserId,
				Iat = authToken.Iat,
				Exp = authToken.Exp,
				Name = authToken.UserAccess.User.Name,
				Email = authToken.UserAccess.User.Email,
				Phone = authToken.UserAccess.User.Phone,
				PhotoUrl = authToken.UserAccess.User.PhotoUrl,
				Slug = authToken.UserAccess.User.Slug
			});
			string payload64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload));
			string jwtData = header64 + "." + payload64;
			string secret = _configuration.GetSection("Jwt").GetSection("Secret").Value!;	
			string signature = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(_hashService.Digest(secret + jwtData)));
			return jwtData + "." + signature;
		}
	}
}

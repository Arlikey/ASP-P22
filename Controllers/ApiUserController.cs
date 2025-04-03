using ASP_P22.Data;
using ASP_P22.Data.Entities;
using ASP_P22.Models;
using ASP_P22.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP_P22.Controllers
{
	[Route("api/user")]
	[ApiController]
	public class ApiUserController(DataAccessor dataAccessor, IStorageService storageService) : ControllerBase
	{
		private readonly IStorageService _storageService = storageService;
		private readonly DataAccessor _dataAccessor = dataAccessor;

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
			model.Data = authToken.Jti;
			return model;
		}
	}
}

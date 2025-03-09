using ASP_P22.Data;
using ASP_P22.Data.Entities;
using ASP_P22.Models;
using ASP_P22.Models.Shop;
using ASP_P22.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ASP_P22.Controllers
{
	[Route("api/category")]
	[ApiController]
	public class ApiCategoryController(DataAccessor dataAccessor, IStorageService storageService) : ControllerBase
	{
		private readonly IStorageService _storageService = storageService;
		private readonly DataAccessor _dataAccessor = dataAccessor;
		[HttpGet]
		public RestResponseModel CategoriesList()
		{
			return new()
			{
				CacheLifetime = 86400,
				Description = "Product Category API: Categories List",
				Manipulations = new()
				{
					Read = "api/category/{id}"
				},
				Meta = new()
				{
					{ "locale", "uk" },
					{ "dataType", "object" }
				},
				Data = _dataAccessor.CategoriesList()
			};
		}
		[HttpGet("{id}")]
		public RestResponseModel CategoryById(string id)
		{
			return new()
			{
				CacheLifetime = 86400,
				Description = "Product Category API: Category By Id",
				Meta = new()
				{
					{ "locale", "uk" },
					{ "dataType", "object" }
				},
				Data = _dataAccessor.CategoryById(id)
			};
		}
	}
}

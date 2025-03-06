using ASP_P22.Data;
using ASP_P22.Data.Entities;
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
		public ShopIndexPageModel CategoriesList()
		{
			return _dataAccessor.CategoriesList();
		}
		[HttpGet("{id}")]
		public ShopCategoryPageModel CategoryById(string id)
		{
			return _dataAccessor.CategoryById(id);
		}
	}
}

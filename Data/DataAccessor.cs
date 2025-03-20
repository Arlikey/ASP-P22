using ASP_P22.Models.Shop;
using ASP_P22.Services.Kdf;
using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Text;

namespace ASP_P22.Data
{
	public class DataAccessor(DataContext dataContext, IHttpContextAccessor httpContextAccessor,
		IKdfService kdfService, IConfiguration configuration)
	{
		private readonly DataContext _dataContext = dataContext;
		private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
		private readonly IKdfService _kdfService = kdfService;
		private readonly IConfiguration _configuration = configuration;
		public Data.Entities.UserAccess? BasicAuthenticate()
		{
			string? authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
			if (string.IsNullOrEmpty(authHeader))
			{
				throw new Exception("Потрібен заголовок авторизації.");
			}
			string authScheme = "Basic ";
			if (!authHeader.StartsWith(authScheme))
			{
				throw new Exception($"Помилка схеми авторизації: потрібна '{authScheme}'.");
			}
			string credentials = authHeader[authScheme.Length..];
			string authData = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(credentials));
			string[] parts = authData.Split(':', 2);
			if (parts.Length != 2)
			{
				throw new Exception($"Облікові дані авторизації неправильні.");
			}
			var access = _dataContext.Accesses.Include(a => a.User).FirstOrDefault(a => a.Login == parts[0]);
			if (access == null)
			{
				throw new Exception($"Авторизацію відхилено.");
			}
			var (iterationCount, dkLength) = KdfSettings();
			string dk1 = _kdfService.Dk(parts[1], access.Salt, iterationCount, dkLength);
			if (dk1 != access.DK)
			{
				throw new Exception($"Авторизацію відхилено.");
			}
			return access;
		}
		public ShopIndexPageModel CategoriesList()
		{
			ShopIndexPageModel model = new()
			{
				Categories = [.. _dataContext.Categories]
			};
			if (model.Categories != null)
			{
				model.Categories = model.Categories.Select(c => c with
				{
					ImagesCsv = c.ImagesCsv == null
						? StoragePrefix + "no-image.jpg"
						: string.Join(',', c.ImagesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
					.Select(i => StoragePrefix + i))
				}).ToList();
			}

			return model;
		}
		public ShopCategoryPageModel CategoryById(string id)
		{
			ShopCategoryPageModel model = new()
			{
				Category = _dataContext
				.Categories
				.Include(c => c.Products)
					.ThenInclude(p => p.Rates)
				.FirstOrDefault(c => c.Slug == id),
				Categories = [.. _dataContext.Categories]
			};
			if (model.Category != null)
			{
				model.Category = model.Category with
				{
					Products = model.Category.Products.Select(p => p with
					{
						ImagesCsv = p.ImagesCsv == null
							? StoragePrefix + "no-image.jpg"
							: string.Join(',', p.ImagesCsv.Split(',', StringSplitOptions.RemoveEmptyEntries)
						.Select(i => StoragePrefix + i)
					)
					}).ToList()
				};
			}

			return model;
		}

		public ShopProductPageModel ProductById(string id)
		{
			string? authUserId = _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;

			ShopProductPageModel model = new()
			{
				Product = _dataContext
				.Products
				.Include(p => p.Category)
					.ThenInclude(c => c.Products)
				.Include(p => p.Rates)
					.ThenInclude(r => r.User)
				.FirstOrDefault(p => p.Slug == id || p.Id.ToString() == id),
				IsUserCanRate = authUserId != null && _dataContext
					.CartDetails
					.Any(cd => (cd.ProductId.ToString() == id || cd.Product.Slug == id) && cd.Cart.UserId.ToString() == authUserId),
				UserRate = authUserId == null ? null : _dataContext.Rates.FirstOrDefault(r => (r.ProductId.ToString() == id || r.Product.Slug == id) && r.UserId.ToString() == authUserId),
				AuthUserId = authUserId,
				Categories = [.. _dataContext.Categories]
			};

			return model;
		}
		private (uint, uint) KdfSettings()
		{
			var kdf = _configuration.GetSection("Kdf");
			return (
				kdf.GetSection("IterationCount").Get<uint>(),
				kdf.GetSection("DkLength").Get<uint>()
			);
		}
		private string StoragePrefix => $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/Storage/Item/";
	}
}

using ASP_P22.Data;
using ASP_P22.Models.Shop;
using ASP_P22.Models.User;
using ASP_P22.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace ASP_P22.Controllers
{
	public class ShopController(DataContext dataContext,
		IStorageService storageService) : Controller
	{
		private readonly IStorageService _storageService = storageService;
		private readonly DataContext _dataContext = dataContext;
		public IActionResult Index()
		{
			ShopIndexPageModel model = new()
			{
				Categories = [.. _dataContext.Categories]
			};
			if (HttpContext.Session.Keys.Contains("productModelErrors"))
			{
				model.Errors = JsonSerializer.Deserialize<Dictionary<string, string>>(
					HttpContext.Session.GetString("productModelErrors")!
				);
				model.ValidationStatus = JsonSerializer.Deserialize<bool>(
					HttpContext.Session.GetString("productModelStatus")!
				);
				HttpContext.Session.Remove("productModelErrors");
				HttpContext.Session.Remove("productModelStatus");
			}
			return View(model);
		}
		public ViewResult Product([FromRoute] string id)
		{
			ShopProductPageModel model = new()
			{
				Product = _dataContext
				.Products
				.Include(p => p.Category).ThenInclude(c => c.Products)
				.FirstOrDefault(p => p.Slug == id || p.Id.ToString() == id),
				Categories = [.. _dataContext.Categories]
			};
			return View(model);
		}
		public ViewResult Category([FromRoute] string id)
		{
			ShopCategoryPageModel model = new()
			{
				Category = _dataContext
				.Categories
				.Include(c => c.Products)
				.FirstOrDefault(c => c.Slug == id),
				Categories = [.. _dataContext.Categories]
			};
			return View(model);
		}
		public RedirectToActionResult AddProduct([FromForm] ShopProductFormModel model)
		{
			(bool? status, Dictionary<string, string> errors) = ValidateShopProductModel(model);

			if (status ?? false)
			{
				string? imagesCsv = null;
				if (model.Images != null)
				{
					imagesCsv = "";
					foreach (IFormFile file in model!.Images)
					{
						imagesCsv += _storageService.Save(file) + ',';
					}
				}
				_dataContext.Products.Add(new Data.Entities.Product
				{
					Id = Guid.NewGuid(),
					Name = model.Name,
					Description = model.Description,
					CategoryId = model.CategoryId,
					Price = model.Price,
					Stock = model.Stock,
					Slug = model.Slug,
					ImagesCsv = imagesCsv
				});
				_dataContext.SaveChanges();
			}
			HttpContext.Session.SetString("productModelErrors",
			JsonSerializer.Serialize(errors));
			HttpContext.Session.SetString("productModelStatus",
			JsonSerializer.Serialize(status));

			return RedirectToAction(nameof(Index));
		}

		private (bool, Dictionary<string, string>) ValidateShopProductModel(ShopProductFormModel? model)
		{
			bool status = true;
			Dictionary<string, string> errors = [];
			if (model == null)
			{
				status = false;
				errors["ModelState"] = "Модель не передано.";
			}
			else
			{
				if (string.IsNullOrEmpty(model.Name))
				{
					status = false;
					errors["ProductName"] = "Назва товару не може бути порожньою.";
				}
				else if(model.Name.Length < 3)
				{
					status = false;
					errors["ProductName"] = "Назва товару повинна мати більше 3 символів.";
				}

				if (string.IsNullOrEmpty(model.Description))
				{
					status = false;
					errors["ProductDescription"] = "Опис товару не може бути порожнім.";
				}
				else if (model.Description.Length < 15)
				{
					status = false;
					errors["ProductDescription"] = "Опис товару повинен мати більше 15 символів.";
				}

				if (model.Price < 0)
				{
					status = false;
					errors["ProductPrice"] = "Ціна товару не може бути від'ємною.";
				}

				if (model.Stock < 0)
				{
					status = false;
					errors["ProductStock"] = "Кількість товару не може бути від'ємною.";
				}

				if (!string.IsNullOrEmpty(model.Slug))
				{
					if (_dataContext.Products.Count(p => p.Slug == model.Slug) > 0)
					{
						status = false;
						errors["ProductSlug"] = "Slug товару вже існує.";
					}
				}

				if (model.Images != null) 
				{
					foreach (var image in model.Images)
					{
						string fileExtension = Path.GetExtension(image.FileName);
						List<string> availableExtensions = [".jpg", ".png", ".webp", ".jpeg"];
						if (!availableExtensions.Contains(fileExtension))
						{
							status = false;
							errors["ProductImages"] = "Файл повинен мати розширення .jpg, .png, .webp, .jpeg.";
							break;
						}
					}
				}
				
			}
			return (status, errors);
		}
	}
}

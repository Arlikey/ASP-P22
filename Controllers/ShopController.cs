﻿using ASP_P22.Data;
using ASP_P22.Models.Shop;
using ASP_P22.Models.User;
using ASP_P22.Services.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
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
		[HttpPut]
		public JsonResult AddToCart([FromRoute] string id)
		{
			string? userId = HttpContext
				.User
				.Claims
				.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?
				.Value;
			if (userId == null)
			{
				return Json(new { status = 401, message = "Unauthorized" });
			}
			Guid uid = Guid.Parse(userId);

			Guid productId;
			try { productId = Guid.Parse(id); }
			catch
			{
				return Json(new { status = 400, message = "UUID required" });
			}
			var product = _dataContext.Products.FirstOrDefault(p => p.Id == productId);
			if (product == null)
			{
				return Json(new { status = 404, message = "Product not found" });
			}

			var cart = _dataContext.Carts.FirstOrDefault(c => c.UserId == uid && c.MomentBuy == null && c.MomentCancel == null);
			if (cart == null)
			{
				cart = new Data.Entities.Cart()
				{
					Id = Guid.NewGuid(),
					MomentOpen = DateTime.Now,
					UserId = uid,
					Price = 0,
				};
				_dataContext.Carts.Add(cart);
			}
			var cd = _dataContext.CartDetails.FirstOrDefault(d => d.CartId == cart.Id && d.ProductId == product.Id);
			if (cd != null)
			{
				cd.Quantity += 1;
				cd.Price += product.Price;
				cart.Price += product.Price;
			}
			else
			{
				cd = new Data.Entities.CartDetail()
				{
					Id = Guid.NewGuid(),
					Moment = DateTime.Now,
					CartId = cart.Id,
					ProductId = productId,
					Price = product.Price,
					Quantity = 1
				};
				cart.Price += product.Price;
				_dataContext.CartDetails.Add(cd);
			}
			_dataContext.SaveChanges();

			return Json(new { status = 201, message = "Created" });
		}
		[HttpPatch]
		public JsonResult ModifyCart([FromRoute] string id, [FromQuery] int delta)
		{
			string? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
			if (userId == null)
			{
				return Json(new { status = 401, message = "Unauthorized" });
			}
			Guid cdId;
			try
			{
				cdId = Guid.Parse(id);
			}
			catch
			{
				return Json(new { status = 400, message = "Id unrecognized" });
			}
			if (delta == 0)
			{
				return Json(new { status = 400, message = "Dummy action" });
			}
			var cartDetail = _dataContext
				.CartDetails
				.Include(cd => cd.Product)
				.Include(cd => cd.Cart)
				.FirstOrDefault(cd => cd.Id == cdId);
			if (cartDetail == null)
			{
				return Json(new { status = 404, message = "Item not found" });
			}

			Guid uid = Guid.Parse(userId);
			if (uid != cartDetail.Cart.UserId)
			{
				return Json(new { status = 403, message = "Resource does not belong to user" });
			}

			if (cartDetail.Quantity + delta < 0)
			{
				return Json(new { status = 422, message = "decrement too large" });
			}

			if (cartDetail.Quantity + delta > cartDetail.Product.Stock)
			{
				return Json(new { status = 406, message = "increment too large" });
			}

			if (cartDetail.Quantity + delta == 0)
			{
				cartDetail.Cart.Price += delta * cartDetail.Product.Price;
				_dataContext.CartDetails.Remove(cartDetail);
			}
			else
			{
				cartDetail.Quantity += delta;
				cartDetail.Price += delta * cartDetail.Product.Price;
				cartDetail.Cart.Price += delta * cartDetail.Product.Price;
			}
			_dataContext.SaveChanges();
			return Json(new { status = 202, message = "Accepted" });
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
				else if (model.Name.Length < 3)
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

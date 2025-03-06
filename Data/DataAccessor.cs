using ASP_P22.Models.Shop;
using Microsoft.EntityFrameworkCore;

namespace ASP_P22.Data
{
	public class DataAccessor(DataContext dataContext)
	{
		private readonly DataContext _dataContext = dataContext;
		public ShopIndexPageModel CategoriesList()
		{
			ShopIndexPageModel model = new()
			{
				Categories = [.. _dataContext.Categories]
			};
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
			return model;
		}
	}
}

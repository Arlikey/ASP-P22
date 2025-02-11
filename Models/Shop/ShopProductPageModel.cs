using ASP_P22.Data.Entities;

namespace ASP_P22.Models.Shop
{
	public class ShopProductPageModel
	{
		public Data.Entities.Product? Product { get; set; }
		public Category[] Categories { get; set; } = [];
	}
}

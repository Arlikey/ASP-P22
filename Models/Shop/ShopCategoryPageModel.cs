using ASP_P22.Data.Entities;

namespace ASP_P22.Models.Shop
{
    public class ShopCategoryPageModel
    {
        public Data.Entities.Category? Category { get; set; }
		public Category[] Categories { get; set; } = [];
	}
}

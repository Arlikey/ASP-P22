using ASP_P22.Data.Entities;

namespace ASP_P22.Models.Shop
{
    public class ShopIndexPageModel
    {
        public Category[] Categories { get; set; } = [];
		public bool? ValidationStatus { get; set; }
		public Dictionary<string, string>? Errors { get; set; }
	}
}

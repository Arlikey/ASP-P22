namespace ASP_P22.Models.Shop
{
	public class ShopProductFormModel
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public Guid CategoryId { get; set; }
		public decimal Price { get; set; }
		public int Stock {  get; set; }
		public IFormFile[] Images { get; set; }
		public string Slug { get; set; }
	}
}

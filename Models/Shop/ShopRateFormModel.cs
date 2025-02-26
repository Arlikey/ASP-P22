namespace ASP_P22.Models.Shop
{
	public class ShopRateFormModel
	{
		public string UserId { get; set; } = null!;
		public string ProductId { get; set; } = null!;
		public string? Comment { get; set; }
		public int? Rating { get; set; }
	}
}

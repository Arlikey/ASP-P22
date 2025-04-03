using System.Text.Json.Serialization;

namespace ASP_P22.Data.Entities
{
	public record Cart
	{
		public Guid Id { get; set; }
		public Guid UserId { get; set; }
		public DateTime MomentOpen { get; set; }
		public DateTime? MomentBuy { get; set; }
		public DateTime? MomentCancel { get; set; }
		public decimal Price { get; set; }

		[JsonIgnore]
		public User User { get; set; }
		public List<CartDetail> CartDetails { get; set; }
	}
}

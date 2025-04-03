using System.Text.Json.Serialization;

namespace ASP_P22.Data.Entities
{
	public record CartDetail
	{
		public Guid Id { get; set; }
		public Guid CartId { get; set; }
		public Guid ProductId { get; set; }
		public DateTime Moment { get; set; }	
		public decimal Price { get; set; }
		public int Quantity { get; set; } = 1;

		[JsonIgnore]
		public Cart Cart { get; set; }
		
		public Product Product { get; set; }	
	}
}

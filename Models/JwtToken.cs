using ASP_P22.Data.Entities;
using System.Text.Json.Serialization;

namespace ASP_P22.Models
{
	public class JwtToken
	{
		public Guid Jti { get; set; }
		public Guid? Sub { get; set; }
		public DateTime Iat { get; set; }
		public DateTime Exp { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string? Phone { get; set; }
		public string? PhotoUrl { get; set; }
		public string Slug { get; set; }
	}
}

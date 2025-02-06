using System.Text.Json.Serialization;

namespace ASP_P22.Data.Entities
{
    public class Category
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string? ImagesCsv { get; set; }
        public string? Slug { get; set; }
        [JsonIgnore]
        public IEnumerable<Product> Products { get; set; } = [];
    }
}

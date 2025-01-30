using System.Text.Json.Serialization;

namespace ASP_P22.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string? WorkPosition { get; set; }
        public string? PhotoUrl { get; set; }
        [JsonIgnore]
        public List<UserAccess> Accesses { get; set; } = [];
    }
}

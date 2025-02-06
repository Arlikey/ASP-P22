using System.Text.Json.Serialization;

namespace ASP_P22.Models.User
{
    public class UserSignUpFormModel
    {
        public string UserName { get; set; } = null!;
        public string UserEmail { get; set; } = null!;
        public string UserLogin { get; set; } = null!;
        public string Password1 { get; set; } = null!;
        public string Password2 { get; set; } = null!;
        public string SlugOption { get; set; }
        public string CustomSlug { get; set; }
        public string UserPhone { get; set; } = null!;
        public string UserPosition { get; set; } = null!;
        [JsonIgnore]
        public IFormFile UserPhoto { get; set; } = null!;
        public string UserPhotoSavedName { get; set; } = null!;
    }
}

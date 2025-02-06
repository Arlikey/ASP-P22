namespace ASP_P22.Models.User
{
    public class UserProfilePageModel
    {
        public bool IsFound { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";


        public string Phone {  get; set; } = "";
        public string MostViewed {  get; set; } = "";
        public string Recent {  get; set; } = "";
        public string Role {  get; set; } = "";
        public string PhotoUrl { get; set; } = "";
    }
}

namespace ASP_P22.Models.User
{
    public class UserSignUpPageModel
    {
        public UserSignUpFormModel? FormModel { get; set; }
        public bool? ValidationStatus { get; set; }
        public Dictionary<string, string>? Errors { get; set; }
        public Data.Entities.User? User { get; set; }
    }
}

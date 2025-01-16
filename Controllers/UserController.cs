using ASP_P22.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ASP_P22.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.Keys.Contains("formModel"))
            {
                ViewData["formModel"] = JsonSerializer.Deserialize<UserSignUpFormModel>(
                    HttpContext.Session.GetString("formModel")!
                );
                HttpContext.Session.Remove("formModel");
            }
            return View();
        }
        public IActionResult SignUp([FromForm] UserSignUpFormModel userSignUpFormModel)
        {
            HttpContext.Session.SetString("formModel",
                JsonSerializer.Serialize(userSignUpFormModel));
            return RedirectToAction("Index");
        }
        public IActionResult Review() 
        {
            if (HttpContext.Session.Keys.Contains("reviewModel"))
            {
                ViewData["reviewModel"] = JsonSerializer.Deserialize<UserReviewFormModel>(
                    HttpContext.Session.GetString("reviewModel")!
                );
                HttpContext.Session.Remove("reviewModel");
            }
            return View();
        }

        public IActionResult LeftReview([FromForm] UserReviewFormModel userReviewFormModel) 
        {
            HttpContext.Session.SetString("reviewModel",
                JsonSerializer.Serialize(userReviewFormModel));
            return RedirectToAction("Review");
        }
    }
}

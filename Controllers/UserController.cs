using ASP_P22.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_P22.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.Keys.Contains("formModel"))
            {
                var userSignUpFormModel  = JsonSerializer.Deserialize<UserSignUpFormModel>(
                    HttpContext.Session.GetString("formModel")!
                );
                var (validationStatus, errors) = ValidateUserSignUpModel(userSignUpFormModel);
                ViewData["formModel"]=userSignUpFormModel;
                ViewData["validationStatus"] = validationStatus;
                ViewData["errors"] = errors;

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
        private (bool, Dictionary<string, string>) ValidateUserSignUpModel(UserSignUpFormModel? userSignUpFormModel) 
        {
            bool status = true;
            Dictionary<string, string> errors = [];

            if(userSignUpFormModel == null)
            {
                status = false;
                errors["ModelState"] = "Модель не передано.";
                return (status, errors);
            }
            if (string.IsNullOrEmpty(userSignUpFormModel.UserName))
            {
                status = false;
                errors["UserName"] = "Ім'я не може бути порожнім.";
            }
            else if(!Regex.IsMatch(userSignUpFormModel.UserName, "^[A-ZА-Я].*"))
            {
                status = false;
                errors["UserName"] = "Ім'я має починатися з великої літери.";
            }


            if (string.IsNullOrEmpty(userSignUpFormModel.UserEmail))
            {
                status = false;
                errors["UserEmail"] = "Email не може бути порожнім.";
            }
            else if (!Regex.IsMatch(userSignUpFormModel.UserEmail, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
            {
                status = false;
                errors["UserEmail"] = "Email не відповідає шаблону.";
            }


            if (string.IsNullOrEmpty(userSignUpFormModel.UserLogin))
            {
                status = false;
                errors["UserLogin"] = "Логін не може бути порожнім.";
            }
            else if (userSignUpFormModel.UserLogin.Contains(':'))
            {
                status = false;
                errors["UserLogin"] = "Логін не повинен містити символ ':'.";
            }


            if (string.IsNullOrEmpty(userSignUpFormModel.Password1))
            {
                status = false;
                errors["Password1"] = "Пароль не може бути порожнім.";
            }
            else if (userSignUpFormModel.Password1.Length < 8 || userSignUpFormModel.Password1.Length > 16)
            {
                status = false;
                errors["Password1"] = "Пароль повинен містити від 8 до 16 символів.";
            } 
            else if (!Regex.IsMatch(userSignUpFormModel.Password1, @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*]).*$"))
            {
                status = false;
                errors["Password1"] = "Пароль повинен містити принаймні одну літеру, одну цифру та один спеціальний символ (!@#$%^&*).";
            }


            if (string.IsNullOrEmpty(userSignUpFormModel.Password2))
            {
                status = false;
                errors["Password2"] = "Пароль не може бути порожнім.";
            }
            else if (string.Compare(userSignUpFormModel.Password1, userSignUpFormModel.Password2) != 0)
            {
                status = false;
                errors["Password2"] = "Паролі не співпадають.";
            }
            return (status, errors);
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

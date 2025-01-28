using ASP_P22.Data;
using ASP_P22.Models.User;
using ASP_P22.Services.Kdf;
using ASP_P22.Services.Random;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_P22.Controllers
{
    public class UserController(DataContext dataContext, IKdfService kdfService,
        IRandomService randomService, IConfiguration configuration) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IRandomService _randomService = randomService;
        private readonly IConfiguration _configuration = configuration;
        public IActionResult Index()
        {
            UserSignUpPageModel pageModel = new();
            if (HttpContext.Session.Keys.Contains("formModel"))
            {
                var userSignUpFormModel  = JsonSerializer.Deserialize<UserSignUpFormModel>(
                    HttpContext.Session.GetString("formModel")!
                );
                pageModel.FormModel = userSignUpFormModel;
                (pageModel.ValidationStatus, pageModel.Errors) = ValidateUserSignUpModel(userSignUpFormModel);

                /*ViewData["formModel"]=userSignUpFormModel;
                ViewData["validationStatus"] = validationStatus;
                ViewData["errors"] = errors;*/
                if (pageModel.ValidationStatus ?? false)
                {
                    // Реєструємо в Базі данних
                    Data.Entities.User user = new()
                    {
                        Id = Guid.NewGuid(),
                        Name = userSignUpFormModel!.UserName,
                        Email = userSignUpFormModel.UserEmail
                    };
                    String salt = _randomService.FileName();
                    var (iterationCount, dkLength) = KdfSettings();
                    Data.Entities.UserAccess access = new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Login = userSignUpFormModel.UserLogin,
                        Salt = salt,
                        DK = _kdfService.Dk(userSignUpFormModel.Password1, salt, iterationCount, dkLength)
                    };
                    _dataContext.Users.Add(user);
                    _dataContext.Accesses.Add(access);
                    _dataContext.SaveChanges();
                    pageModel.User = user;
                }
                HttpContext.Session.Remove("formModel");
            }
            return View(pageModel);
        }
        [HttpGet]
        public JsonResult Authenticate()
        {
            string authHeader = Request.Headers.Authorization.ToString();
            if (string.IsNullOrEmpty(authHeader))
            {
                return AuthError("Потрібен заголовок авторизації.");
            }
            string authScheme = "Basic ";
            if( ! authHeader.StartsWith(authScheme))
            {
                return AuthError($"Помилка схеми авторизації: потрібна '{authScheme}'.");
            }
            string credentials = authHeader[authScheme.Length..];
            string authData = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(credentials));
            string[] parts = authData.Split(':', 2);
            if (parts.Length != 2)
            {
                return AuthError($"Облікові дані авторизації неправильні.");
            }
            var access = _dataContext.Accesses.Include(a => a.User).FirstOrDefault(a => a.Login == parts[0]);
            if(access == null)
            {
                return AuthError($"Авторизацію відхилено.");
            }
            var (iterationCount, dkLength) = KdfSettings();
            string dk1 = _kdfService.Dk(parts[1], access.Salt, iterationCount, dkLength);
            if(dk1 != access.DK)
            {
                return AuthError($"Авторизацію відхилено.");
            }
            HttpContext.Session.SetString("authUser",
                JsonSerializer.Serialize(access.User));
            return Json("Ok");
        }  

        private JsonResult AuthError(string message)
        {
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Json(message);
        }

        private (uint, uint) KdfSettings()
        {
            var kdf = _configuration.GetSection("Kdf");
            return (
                kdf.GetSection("IterationCount").Get<uint>(), 
                kdf.GetSection("DkLength").Get<uint>()
            );
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
            else if (_dataContext.Accesses.Count(a => a.Login == userSignUpFormModel.UserLogin) > 0) 
            {
                status = false;
                errors["UserLogin"] = "Користувач з таким логіном вже існує.";
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

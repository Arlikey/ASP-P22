using ASP_P22.Data;
using ASP_P22.Models.User;
using ASP_P22.Services.Kdf;
using ASP_P22.Services.Random;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ASP_P22.Controllers
{
    public class UserController(
        DataContext dataContext, 
        IKdfService kdfService,
        IRandomService randomService, 
        IConfiguration configuration,
        ILogger<UserController> logger) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IRandomService _randomService = randomService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<UserController> _logger = logger;
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
                        Email = userSignUpFormModel.UserEmail,

                        Phone = userSignUpFormModel.UserPhone,
                        WorkPosition = userSignUpFormModel.UserPosition,
                        PhotoUrl = userSignUpFormModel.UserPhotoSavedName
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
        public ViewResult Profile()
        {
            UserProfilePageModel pageModel = new()
            {
                PhotoUrl = "https://img.icons8.com/bubbles/100/000000/user.png",
                Name = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "",
                Email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "",
                Phone = "380992019714",
                Recent = "Razor",
                MostViewed = "ASP",
                Role = "Web Designer"
            };
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
        public RedirectToActionResult SignUp([FromForm] UserSignUpFormModel formModel)
        {
            if(formModel.UserPhoto != null)
            {
                formModel.UserPhotoSavedName = formModel.UserPhoto.FileName;
                _logger.LogInformation("File uploaded {name}", formModel.UserPhoto.FileName);
            }
            
            HttpContext.Session.SetString("formModel",
                JsonSerializer.Serialize(formModel));
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
        public RedirectToActionResult LeftReview([FromForm] UserReviewFormModel userReviewFormModel) 
        {
            HttpContext.Session.SetString("reviewModel",
                JsonSerializer.Serialize(userReviewFormModel));
            return RedirectToAction("Review");
        }
        private (bool, Dictionary<string, string>) ValidateUserSignUpModel(UserSignUpFormModel? formModel) 
        {
            bool status = true;
            Dictionary<string, string> errors = [];

            if(formModel == null)
            {
                status = false;
                errors["ModelState"] = "Модель не передано.";
                return (status, errors);
            }
            if (string.IsNullOrEmpty(formModel.UserName))
            {
                status = false;
                errors["UserName"] = "Ім'я не може бути порожнім.";
            }
            else if(!Regex.IsMatch(formModel.UserName, "^[A-ZА-Я].*"))
            {
                status = false;
                errors["UserName"] = "Ім'я має починатися з великої літери.";
            }


            if (string.IsNullOrEmpty(formModel.UserEmail))
            {
                status = false;
                errors["UserEmail"] = "Email не може бути порожнім.";
            }
            else if (!Regex.IsMatch(formModel.UserEmail, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
            {
                status = false;
                errors["UserEmail"] = "Email не відповідає шаблону.";
            }


            if (!string.IsNullOrEmpty(formModel.UserPhone))
            {
                if (!Regex.IsMatch(formModel.UserPhone, @"^\+?\d{10,13}$"))
                {
                    status = false;
                    errors["UserPhone"] = "Номер телефону не відповідає стандартному шаблону.";
                }
            }


            if (!string.IsNullOrEmpty(formModel.UserPosition))
            {
                if (formModel.UserPosition.Length < 3)
                {
                    status = false;
                    errors["UserPosition"] = "Посада не може бути коротшою за 3 символи.";
                }
                else if (char.IsDigit(formModel.UserPosition[0]))
                {
                    status = false;
                    errors["UserPosition"] = "Посада не повинна починатися з цифри.";
                }
                else if (Regex.IsMatch(formModel.UserPosition, @"[^A-Za-zА-Яа-я0-9\s-]"))
                {
                    status = false;
                    errors["UserPosition"] = "Посада не повинна містити спеціальні символи (окрім '-').";
                }
            }

            if (!string.IsNullOrEmpty(formModel.UserPhotoSavedName))
            {
                string fileExtension = Path.GetExtension(formModel.UserPhotoSavedName);
                List<string> availableExtensions = [".jpg", ".png", ".webp", ".jpeg"];
                if (!availableExtensions.Contains(fileExtension))
                {
                    status = false;
                    errors["UserPhoto"] = "Файл повинен мати розширення .jpg, .png, .webp, .jpeg.";
                }
            }


            if (string.IsNullOrEmpty(formModel.UserLogin))
            {
                status = false;
                errors["UserLogin"] = "Логін не може бути порожнім.";
            }
            else if (formModel.UserLogin.Contains(':'))
            {
                status = false;
                errors["UserLogin"] = "Логін не повинен містити символ ':'.";
            }
            else if (_dataContext.Accesses.Count(a => a.Login == formModel.UserLogin) > 0) 
            {
                status = false;
                errors["UserLogin"] = "Користувач з таким логіном вже існує.";
            }


            if (string.IsNullOrEmpty(formModel.Password1))
            {
                status = false;
                errors["Password1"] = "Пароль не може бути порожнім.";
            }
            else if (formModel.Password1.Length < 8 || formModel.Password1.Length > 16)
            {
                status = false;
                errors["Password1"] = "Пароль повинен містити від 8 до 16 символів.";
            } 
            else if (!Regex.IsMatch(formModel.Password1, @"^(?=.*[A-Za-z])(?=.*\d)(?=.*[!@#$%^&*]).*$"))
            {
                status = false;
                errors["Password1"] = "Пароль повинен містити принаймні одну літеру, одну цифру та один спеціальний символ (!@#$%^&*).";
            }

            if (string.IsNullOrEmpty(formModel.Password2))
            {
                status = false;
                errors["Password2"] = "Пароль не може бути порожнім.";
            }
            else if (string.Compare(formModel.Password1, formModel.Password2) != 0)
            {
                status = false;
                errors["Password2"] = "Паролі не співпадають.";
            }
            return (status, errors);
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
    }
}

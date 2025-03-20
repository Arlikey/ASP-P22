using ASP_P22.Data;
using ASP_P22.Models.User;
using ASP_P22.Services.Kdf;
using ASP_P22.Services.Random;
using ASP_P22.Services.Slugify;
using ASP_P22.Services.Storage;
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
        ILogger<UserController> logger,
        IStorageService storageService,
        ISlugifyService slugifyService,
        DataAccessor dataAccessor) : Controller
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;
        private readonly IConfiguration _configuration = configuration;
        private readonly IRandomService _randomService = randomService;
        private readonly ILogger<UserController> _logger = logger;
        private readonly IStorageService _storageService = storageService;
        private readonly ISlugifyService _slugifyService = slugifyService;
		private readonly DataAccessor _dataAccessor = dataAccessor;
		public IActionResult Index()
        {
            UserSignUpPageModel pageModel = new();
            if (HttpContext.Session.Keys.Contains("formModel"))
            {
                var formModel = JsonSerializer.Deserialize<UserSignUpFormModel>(
                    HttpContext.Session.GetString("formModel")!
                );
                pageModel.FormModel = formModel;
                (pageModel.ValidationStatus, pageModel.Errors) = ValidateUserSignUpModel(formModel);

                /*ViewData["formModel"]=userSignUpFormModel;
                ViewData["validationStatus"] = validationStatus;
                ViewData["errors"] = errors;*/
                if (pageModel.ValidationStatus ?? false)
                {
                    string slug = formModel.UserLogin;
                    if (formModel.SlugOption == "name")
                    {
                        slug = _slugifyService.GenerateSlug(slug);
                    }
                    else if (formModel.SlugOption == "custom")
                    {
                        slug = formModel.CustomSlug;
                    }
                    // Реєструємо в Базі данних
                    Data.Entities.User user = new()
                    {
                        Id = Guid.NewGuid(),
                        Name = formModel!.UserName,
                        Email = formModel.UserEmail,

                        Phone = formModel.UserPhone,
                        WorkPosition = formModel.UserPosition,
                        PhotoUrl = formModel.UserPhotoSavedName,

                        Slug = formModel.UserLogin
                    };
                    String salt = _randomService.FileName();
                    var (iterationCount, dkLength) = KdfSettings();
                    Data.Entities.UserAccess access = new()
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        Login = formModel.UserLogin,
                        Salt = salt,
                        DK = _kdfService.Dk(formModel.Password1, salt, iterationCount, dkLength)
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
        public ViewResult Cart(string? id)
        {
            UserCartPageModel model = new();
            string? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            if (userId != null)
            {
                Guid uid = Guid.Parse(userId);
                if(id == null)
                {
                    model.ActiveCart = _dataContext
                    .Carts
                    .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Product)
                    .FirstOrDefault(c => c.UserId == uid && c.MomentCancel == null && c.MomentBuy == null);
                }
                else
                {
                    model.ActiveCart = _dataContext
                    .Carts
                    .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Product)
                    .FirstOrDefault(c => c.Id.ToString() == id);
                }
            }

            return View(model);
        }
        public ViewResult Profile([FromRoute] string id)
        {
            string? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            var authUser = userId == null ? null : _dataContext
                .Users
                .Include(u => u.Carts)
                    .ThenInclude(c => c.CartDetails)
                .FirstOrDefault(u => u.Id.ToString() == userId);

            UserProfilePageModel pageModel;
            var profileUser = _dataContext.Users.FirstOrDefault(u => u.Slug == id);
            bool isOwner = authUser?.Slug == profileUser?.Slug;

            if (profileUser == null)
            {
                pageModel = new() { IsFound = false };
            }
            else
            {
                pageModel = new()
                {
                    IsFound = false,
                    PhotoUrl = "/Storage/Item/" + profileUser.PhotoUrl,
                    /*Name = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value ?? "",
                    Email = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? "",*/
                    Name = profileUser.Name,
                    Email = profileUser.Email,
                    Phone = profileUser.Phone ?? "",
                    Recent = "Razor",
                    MostViewed = "ASP",
                    Role = profileUser.WorkPosition ?? "",
                    IsOwner = isOwner,
                    Carts = isOwner ? authUser!.Carts.OrderByDescending(c => c.MomentOpen).ToList() : [],
                };
            }
            return View(pageModel);
        }
        [HttpGet]
        public JsonResult Authenticate()
        {
            try
            {
				var access = _dataAccessor.BasicAuthenticate();
				HttpContext.Session.SetString("authUser",
					JsonSerializer.Serialize(access.User));
				return Json("Ok");
			}
            catch (Exception ex)
            {
                return AuthError(ex.Message);
            }
            
        }
        public RedirectToActionResult SignUp([FromForm] UserSignUpFormModel formModel)
        {
            if (formModel.UserPhoto != null && formModel.UserPhoto.Length != 0)
            {
                _logger.LogInformation("File uploaded {name}", formModel.UserPhoto.FileName);

                formModel.UserPhotoSavedName = _storageService.Save(formModel.UserPhoto);
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

            if (formModel == null)
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
            else if (!Regex.IsMatch(formModel.UserName, "^[A-ZА-Я].*"))
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

            if (formModel.SlugOption == "custom")
            {
                if (string.IsNullOrEmpty(formModel.CustomSlug))
                {
                    status = false;
                    errors["CustomSlug"] = "Slug не може бути порожнім.";
                }
                else if (_dataContext.Users.Count(a => a.Slug == formModel.CustomSlug) > 0)
                {
                    status = false;
                    errors["UserPosition"] = "Slug вже існує.";
                }
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

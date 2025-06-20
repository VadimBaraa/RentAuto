using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RentAutoWeb.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;



namespace RentAutoWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginController> _logger;

        public LoginController(AppDbContext context, SignInManager<User> signInManager, ILogger<LoginController> logger)
        {
            _context = context;
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpPost]
        [Route("/Login/Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            _logger.LogInformation("Начало Login");

            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                    if (user != null)
                    {
                        // Вход с куками (сохраняется между сессиями)
                        var result = await _signInManager.PasswordSignInAsync(user.UserName, model.Password, isPersistent: true, lockoutOnFailure: false);

                        if (result.Succeeded)
                        {
                            HttpContext.Session.SetString("UserId", user.Id.ToString());
                            HttpContext.Session.SetString("UserName", user.FirstName);

                            // ✅ Сохраняем имя в куки
                            Response.Cookies.Append("UserName", user.FirstName, new CookieOptions
                            {
                                Expires = DateTimeOffset.UtcNow.AddDays(3),
                                HttpOnly = false
                            });

                            _logger.LogInformation("Успешный вход");
                            return Json(new { message = "Вход выполнен успешно!", userName = user.FirstName });
                        }
                    }

                    var errorResponse = new ErrorResponse();
                    errorResponse.Errors.Add("Login", new List<string> { "Неверный email или пароль" });
                    _logger.LogWarning("Неверный email или пароль");
                    return BadRequest(errorResponse);
                }
                else
                {
                    var errorResponse = new ErrorResponse();
                    foreach (var modelStateKey in ModelState.Keys)
                    {
                        var modelStateVal = ModelState[modelStateKey];
                        foreach (var error in modelStateVal.Errors)
                        {
                            if (!errorResponse.Errors.ContainsKey(modelStateKey))
                            {
                                errorResponse.Errors[modelStateKey] = new List<string>();
                            }
                            errorResponse.Errors[modelStateKey].Add(error.ErrorMessage);
                        }
                    }

                    _logger.LogWarning("Ошибка валидации модели");
                    return BadRequest(errorResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в Login: {ex.Message}");
                var errorResponse = new ErrorResponse();
                errorResponse.Errors.Add("Server", new List<string> { "Произошла ошибка на сервере. Попробуйте позже." });
                return BadRequest(errorResponse);
            }
        }

        [HttpPost]
        [Route("/Login/Logout")]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Начало Logout");

            try
            {
                await _signInManager.SignOutAsync();
                HttpContext.Session.Clear();
                
                // ❌ Удаляем куку
                Response.Cookies.Delete("UserName");

                _logger.LogInformation("Успешный выход");
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в Logout: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }



        // Модель для возврата ошибок
        public class ErrorResponse
        {
            public Dictionary<string, List<string>> Errors { get; set; } = new();
        }


    }
}

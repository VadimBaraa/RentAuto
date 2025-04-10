using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RentAutoWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging; // Добавляем using

namespace RentAutoWeb.Controllers
{
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<LoginController> _logger; // Добавляем ILogger

        // Изменяем конструктор
        public LoginController(AppDbContext context, SignInManager<User> signInManager, ILogger<LoginController> logger) // Добавляем ILogger
        {
            _context = context;
            _signInManager = signInManager;
            _logger = logger; // Инициализируем ILogger
        }

        [HttpPost]
        [Route("/Login/Login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            _logger.LogInformation("Начало Login"); // Добавляем логирование
            try
            {
                if (ModelState.IsValid)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                    if (user != null)
                    {
                        // Используем SignInManager для проверки пароля
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                        if (result.Succeeded)
                        {
                            // Пользователь найден и пароль верный
                            HttpContext.Session.SetString("UserId", user.Id.ToString());
                            _logger.LogInformation("Успешный вход"); // Добавляем логирование
                            return Json(new { message = "Вход выполнен успешно!", userName = user.FirstName });
                        }
                    }

                    // Пользователь не найден или пароль неверный
                    var errorResponse = new ErrorResponse();
                    errorResponse.Errors.Add("Login", new List<string> { "Неверный email или пароль" });
                    _logger.LogWarning("Неверный email или пароль"); // Добавляем логирование
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
                    _logger.LogWarning("Ошибка валидации"); // Добавляем логирование
                    return BadRequest(errorResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в Login: {ex.Message}"); // Добавляем логирование
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("/Login/Logout")]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("Начало Logout"); // Добавляем логирование
            try
            {
                await _signInManager.SignOutAsync(); // Используем SignOutAsync для выхода
                HttpContext.Session.Remove("UserId");
                _logger.LogInformation("Успешный выход"); // Добавляем логирование
                return Json(new { message = "Выход выполнен успешно!" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в Logout: {ex.Message}"); // Добавляем логирование
                return BadRequest(ex.Message);
            }
        }
    }
}

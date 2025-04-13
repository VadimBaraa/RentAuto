using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RentAutoWeb.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
                        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                        if (result.Succeeded)
                        {
                            HttpContext.Session.SetString("UserId", user.Id.ToString());
                            HttpContext.Session.SetString("UserName", user.FirstName);
                            _logger.LogInformation("Успешный вход");
                            return Json(new { message = "Вход выполнен успешно!", userName = user.FirstName });
                        }
                    }

                    // Ошибка авторизации
                    var errorResponse = new ErrorResponse();
                    errorResponse.Errors.Add("Login", new List<string> { "Неверный email или пароль" });
                    _logger.LogWarning("Неверный email или пароль");
                    return BadRequest(errorResponse);
                }
                else
                {
                    // Ошибки валидации модели
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
                HttpContext.Session.Remove("UserId");
                HttpContext.Session.Remove("UserName");

                _logger.LogInformation("Успешный выход");

                // После выхода возвращаем на главную
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в Logout: {ex.Message}");

                var errorResponse = new ErrorResponse();
                errorResponse.Errors.Add("Server", new List<string> { "Произошла ошибка при выходе из системы." });

                // В случае ошибки можно тоже вернуть на главную или показать ошибку
                return RedirectToAction("Index", "Home");
            }
        }

    }

    // Модель для возврата ошибок
    public class ErrorResponse
    {
        public Dictionary<string, List<string>> Errors { get; set; } = new();
    }

    // Модель логина
    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}

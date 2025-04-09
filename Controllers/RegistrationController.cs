using Microsoft.AspNetCore.Mvc;
using RentAutoWeb.Models;
using RentAutoWeb.Models.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace RentAutoWeb.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager; // Внедряем UserManager
        private readonly ILogger<RegistrationController> _logger;

        // Изменяем конструктор
        public RegistrationController(AppDbContext context, UserManager<User> userManager, ILogger<RegistrationController> logger)
        {
            _context = context;
            _userManager = userManager; // Инициализируем UserManager
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Registration()
        {
            _logger.LogInformation("Вызван метод Registration");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationStep1([FromBody] RegisterStep1ViewModel model)
        {
            _logger.LogInformation("Начало RegistrationStep1");
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Ошибка валидации в RegistrationStep1");
                    return ValidationProblem(ModelState);
                }
                HttpContext.Session.SetString("RegisterStep1", JsonSerializer.Serialize(model));
                _logger.LogInformation("Успешное завершение RegistrationStep1");
                return Ok(new { message = "Данные успешно сохранены" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в RegistrationStep1: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationStep2([FromBody] RegisterStep2ViewModel model)
        {
            _logger.LogInformation("Начало RegistrationStep2");
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Ошибка валидации в RegistrationStep2");
                    return ValidationProblem(ModelState);
                }

                var step1DataJson = HttpContext.Session.GetString("RegisterStep1");
                if (string.IsNullOrEmpty(step1DataJson))
                {
                    _logger.LogWarning("Step 1 data not found в RegistrationStep2");
                    return BadRequest("Step 1 data not found.");
                }

                HttpContext.Session.SetString("RegisterStep2", JsonSerializer.Serialize(model));
                _logger.LogInformation("Успешное завершение RegistrationStep2");
                return Ok(new { message = "Данные успешно сохранены" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в RegistrationStep2: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationStep3(RegisterStep3ViewModel model)
        {
            _logger.LogInformation("Начало RegistrationStep3");
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Ошибка валидации в RegistrationStep3");
                    return ValidationProblem(ModelState);
                }

                var step1DataJson = HttpContext.Session.GetString("RegisterStep1");
                var step2DataJson = HttpContext.Session.GetString("RegisterStep2");

                if (string.IsNullOrEmpty(step1DataJson) || string.IsNullOrEmpty(step2DataJson))
                {
                    _logger.LogWarning("Previous steps data not found в RegistrationStep3");
                    return BadRequest("Previous steps data not found.");
                }
                RegisterStep1ViewModel step1Model = JsonSerializer.Deserialize<RegisterStep1ViewModel>(step1DataJson);
                RegisterStep2ViewModel step2Model = JsonSerializer.Deserialize<RegisterStep2ViewModel>(step2DataJson);

                User newUser = new User
                {
                    LastName = step1Model.LastName,
                    FirstName = step1Model.FirstName,
                    MiddleName = step1Model.MiddleName,
                    Email = step1Model.Email,
                    PhoneNumber = step1Model.Phone,
                    UserName = step1Model.Email 
                };

                // Используем UserManager для создания пользователя и хеширования пароля
                var result = await _userManager.CreateAsync(newUser, step1Model.Password);

                if (!result.Succeeded)
                {
                    // Обработка ошибок создания пользователя
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return BadRequest(ModelState); // Возвращаем ошибки
                }

                UserAdditionalInfo userAdditionalInfo = new UserAdditionalInfo
                {
                    PassportData = step2Model.PassportData,
                    DriverLicense = step2Model.DriverLicense,
                    UserId = newUser.Id,
                };

                // Save files
                if (model.PassportPhoto != null && model.PassportPhoto.Length > 0)
                {
                    var passportFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.PassportPhoto.FileName);
                    var passportFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", passportFileName);
                    using (var stream = new FileStream(passportFilePath, FileMode.Create))
                    {
                        await model.PassportPhoto.CopyToAsync(stream);
                    }
                    userAdditionalInfo.PassportPhotoPath = "/uploads/" + passportFileName;
                }
                if (model.DriverLicensePhoto != null && model.DriverLicensePhoto.Length > 0)
                {
                    var driverLicenseFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.DriverLicensePhoto.FileName);
                    var driverLicenseFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", driverLicenseFileName);
                    using (var stream = new FileStream(driverLicenseFilePath, FileMode.Create))
                    {
                        await model.DriverLicensePhoto.CopyToAsync(stream);
                    }
                    userAdditionalInfo.DriverLicensePhotoPath = "/uploads/" + driverLicenseFileName;
                }
                _context.UserAdditionalInfos.Add(userAdditionalInfo);
                await _context.SaveChangesAsync();

                HttpContext.Session.Clear();
                _logger.LogInformation("Успешное завершение RegistrationStep3");
                return Ok(new { message = "Регистрация завершена" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в RegistrationStep3: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }
    }
}

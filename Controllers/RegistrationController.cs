using Microsoft.AspNetCore.Mvc;
using RentAutoWeb.Models;
using RentAutoWeb.Models.ViewModels;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;



namespace RentAutoWeb.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager; // Внедряем UserManager
        private readonly ILogger<RegistrationController> _logger;

        private readonly SignInManager<User> _signInManager;

        // Изменяем конструктор
        public RegistrationController(AppDbContext context, UserManager<User> userManager, SignInManager<User> signInManager, ILogger<RegistrationController> logger)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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

                    // Собираем все ошибки из ModelState в удобный для клиента формат
                    var errors = ModelState
                        .Where(ms => ms.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return BadRequest(errors); // Возвращаем ошибки с русскими сообщениями
                }

                // Сохраняем данные первого шага в сессии
                HttpContext.Session.SetString("RegisterStep1", JsonSerializer.Serialize(model));
                _logger.LogInformation("Успешное завершение RegistrationStep1");

                return Ok(new { message = "Данные успешно сохранены" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в RegistrationStep1: {ex.Message}");
                return BadRequest(new { error = "Произошла ошибка на сервере" });
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
                    return BadRequest(ModelState); // Возвращаем ошибки
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
                    var errors = ModelState
                        .Where(kvp => kvp.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                        );
                    return BadRequest(errors);
                }

                var step1Json = HttpContext.Session.GetString("RegisterStep1");
                var step2Json = HttpContext.Session.GetString("RegisterStep2");

                if (string.IsNullOrEmpty(step1Json) || string.IsNullOrEmpty(step2Json))
                {
                    _logger.LogWarning("Данные шагов 1 или 2 отсутствуют");
                    return BadRequest("Не найдены данные регистрации. Пожалуйста, начните заново.");
                }

                var step1 = JsonSerializer.Deserialize<RegisterStep1ViewModel>(step1Json);
                var step2 = JsonSerializer.Deserialize<RegisterStep2ViewModel>(step2Json);

                // Сохраняем фото
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var passportPath = Path.Combine("uploads", Guid.NewGuid() + Path.GetExtension(model.PassportPhoto.FileName));
                using (var stream = new FileStream(Path.Combine("wwwroot", passportPath), FileMode.Create))
                {
                    await model.PassportPhoto.CopyToAsync(stream);
                }

                var licensePath = Path.Combine("uploads", Guid.NewGuid() + Path.GetExtension(model.DriverLicensePhoto.FileName));
                using (var stream = new FileStream(Path.Combine("wwwroot", licensePath), FileMode.Create))
                {
                    await model.DriverLicensePhoto.CopyToAsync(stream);
                }

                // Создаём нового пользователя
                var user = new User
                {
                    UserName = step1.Email,
                    Email = step1.Email,
                    FirstName = step1.FirstName,
                    LastName = step1.LastName,
                    MiddleName = step1.MiddleName,
                    PhoneNumber = step1.Phone,
                    PassportData = step2.PassportData,
                    DriverLicense = step2.DriverLicense,
                    PassportPhotoPath = passportPath,
                    DriverLicensePhotoPath = licensePath
                };

                var result = await _userManager.CreateAsync(user, step1.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.ToDictionary(
                        e => e.Code,
                        e => new List<string> { e.Description }
                    );
                    _logger.LogWarning("Ошибка при создании пользователя: {@Errors}", errors);
                    return BadRequest(errors);
                }

                await _userManager.AddToRoleAsync(user, "User");    
                await _signInManager.SignInAsync(user, isPersistent: true);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FirstName ?? user.UserName)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                HttpContext.Session.SetString("UserName", user.UserName);
                _logger.LogInformation("Пользователь успешно зарегистрирован и вошёл в систему: {Email}", step1.Email);

                return Json(new { 
                    message = "Регистрация успешно завершена. Спасибо!", 
                    redirectUrl = Url.Action("Index", "Home") // или куда нужно
                });

                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ошибка в RegistrationStep3: {ex.Message}");
                return BadRequest(new { message = "Произошла ошибка при регистрации. Попробуйте позже." });
            }
        }

    }
}

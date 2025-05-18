using Microsoft.AspNetCore.Mvc;
using RentAutoWeb.Models;
using RentAutoWeb.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace RentAutoWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly YandexLocatorService _yandexLocatorService;
        private readonly GenerateRandomCoordinates _generateRandomCoordinates;
        private readonly UserManager<User> _userManager;
        private readonly WeatherService _weatherService;

        public HomeController(AppDbContext context, YandexLocatorService yandexLocatorService, GenerateRandomCoordinates generateRandomCoordinates, UserManager<User> userManager, WeatherService weatherService)
        {
            _context = context;
            _yandexLocatorService = yandexLocatorService;
            _generateRandomCoordinates = generateRandomCoordinates;
            _userManager = userManager;
            _weatherService = weatherService;
        }

        public async Task<IActionResult> Index(double? lat, double? lon)
        {
            if (lat.HasValue && lon.HasValue)
            {
                HttpContext.Session.SetDouble("Latitude", lat.Value);
                HttpContext.Session.SetDouble("Longitude", lon.Value);
            }

            var centerLat = HttpContext.Session.GetDouble("Latitude") ?? 51.655;
            var centerLon = HttpContext.Session.GetDouble("Longitude") ?? 39.180;

            ViewBag.CenterLat = centerLat;
            ViewBag.CenterLon = centerLon;

            await GetWeatherAsync();

            // Просто перенаправляем на Auto, поскольку всю логику мы теперь перенесли туда
            return View(); ;
        }

        public async Task<IActionResult> Auto()
        {
            // Получаем координаты из сессии или используем координаты Воронежа по умолчанию
            var centerLat = HttpContext.Session.GetDouble("Latitude") ?? 51.655;
            var centerLon = HttpContext.Session.GetDouble("Longitude") ?? 39.180;

            ViewBag.CenterLat = centerLat;
            ViewBag.CenterLon = centerLon;

            // Получаем данные о погоде
            var weather = await _weatherService.GetCurrentWeatherAsync(centerLat, centerLon);

            double temp = 0;
            string condition = "";

            if (weather != null)
            {
                var weatherMain = weather.weather.FirstOrDefault();
                temp = weather.main.temp;
                condition = weatherMain?.main?.ToLower() ?? "";

                ViewBag.Temp = Math.Round(temp);
                ViewBag.WeatherMain = weatherMain?.main ?? "Unknown";
                ViewBag.WeatherDesc = weatherMain?.description ?? "";
                ViewBag.WeatherIconCode = weatherMain?.icon ?? "01d";
            }
            else
            {
                ViewBag.Temp = "?";
                ViewBag.WeatherMain = "Нет данных";
                ViewBag.WeatherDesc = "";
                ViewBag.WeatherIconCode = "01d";
            }

            // Определяем тип погоды на основе данных API
            bool isBadWeather = condition.Contains("rain") || condition.Contains("snow") || (temp < 0 && !condition.Contains("clear"));
            bool isGoodWeather = condition.Contains("clear") && temp >= 20;
            bool isNeutralWeather = condition.Contains("cloud") || condition.Contains("clear");


            var cars = _context.Cars.ToList();

            // Присвоим случайные координаты, если они не установлены
            foreach (var car in cars)
            {
                if (car.Latitude == 0 && car.Longitude == 0)
                {
                    _generateRandomCoordinates.Generate(car);
                }
            }

            var carIds = cars.Select(c => c.Id).ToList();

            // Загружаем только активные или ожидающие оплаты аренды
            var rentals = _context.Rentals
                .Where(r => carIds.Contains(r.CarId) &&
                            (r.Status == RentalStatus.Active || r.Status == RentalStatus.PendingPayment))
                .Include(r => r.Car)  // Добавляем, чтобы избежать проблем с lazy loading
                .GroupBy(r => r.CarId)
                .Select(g => g.OrderByDescending(r => r.StartDate).FirstOrDefault())
                .ToList();

            // Создаем модель представления с рекомендациями
            var model = cars.Select(car =>
            {
                bool isRec = IsCarRecommended(car, isBadWeather, isGoodWeather, isNeutralWeather);
                string hint = null;

                if (isRec)
                {
                    hint = GetRecommendationHint(isBadWeather, isGoodWeather, isNeutralWeather);
                    Console.WriteLine($"Car: {car.Id}, Category: {car.Category}, Hint: {hint}");
                }

                return new CarWithRentalStatusViewModel
                {
                    Car = car,
                    Rental = rentals.FirstOrDefault(r => r.CarId == car.Id),
                    IsRecommended = isRec,
                    RecommendationHint = hint
                };
            }).ToList();

            return View(model);
        }

        public IActionResult Add()
        {
            return View();
        }

        private string GetRecommendationHint(bool isBadWeather, bool isGoodWeather, bool isNeutralWeather)
        {
            if (isBadWeather)
                return "Рекомендуется автомобиль с хорошим сцеплением: кроссовер или внедорожник.";
            if (isGoodWeather)
                return "Рекомендуются спортивные модели или кабриолеты.";
            if (isNeutralWeather)
                return "Подходят седаны и хэтчбеки.";
            return "Нет особых рекомендаций";
        }

        private bool IsCarRecommended(Car car, bool isBadWeather, bool isGoodWeather, bool isNeutralWeather)
        {
            if (car.Category == null)
                return false;

            var category = car.Category.ToLower();

            if (isBadWeather)
            {
                return category.Contains("кроссовер") || category.Contains("внедорожник");
            }
            else if (isGoodWeather)
            {
                return category.Contains("спортивный") || category.Contains("кабриолет");
            }
            else if (isNeutralWeather)
            {
                return category.Contains("седан") || category.Contains("хэтчбек");
            }
            else
            {
                return false;
            }
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> PersonalCabinet()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var rentals = await _context.Rentals
                .Include(r => r.Car)
                .Where(r => r.UserId == user.Id)
                .ToListAsync();

            return View(rentals);
        }

        public IActionResult About()
        {
            return View(); // Возвращает представление "About.cshtml" из папки "Home"
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            var activeStatuses = new[] { RentalStatus.Active, RentalStatus.PendingPayment };

            var rentals = _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.User)
                .Where(r => activeStatuses.Contains(r.Status))
                .ToList();

            return View(rentals);
        }

        [HttpGet]
        public async Task<IActionResult> GetLocation([FromQuery] double lat, [FromQuery] double lon)
        {
            try
            {
                var address = await _yandexLocatorService.GetAddressAsync(lat, lon);
                return Json(new
                {
                    success = true,
                    address = address
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        private async Task GetWeatherAsync()
        {
            var centerLat = HttpContext.Session.GetDouble("Latitude") ?? 51.655;
            var centerLon = HttpContext.Session.GetDouble("Longitude") ?? 39.180;

            ViewBag.CenterLat = centerLat;
            ViewBag.CenterLon = centerLon;

            var weather = await _weatherService.GetCurrentWeatherAsync(centerLat, centerLon);

            if (weather != null)
            {
                var weatherMain = weather.weather.FirstOrDefault();
                double temp = weather.main.temp;

                ViewBag.Temp = Math.Round(temp);
                ViewBag.WeatherMain = weatherMain?.main ?? "Unknown";
                ViewBag.WeatherDesc = weatherMain?.description ?? "";
                ViewBag.WeatherIconCode = weatherMain?.icon ?? "01d";
            }
            else
            {
                ViewBag.Temp = "?";
                ViewBag.WeatherMain = "Нет данных";
                ViewBag.WeatherDesc = "";
                ViewBag.WeatherIconCode = "01d";
            }
        }

    }
}
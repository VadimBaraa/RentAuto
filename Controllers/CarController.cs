using Microsoft.AspNetCore.Mvc;
using RentAutoWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;


namespace RentAutoWeb.Controllers
{
    public class CarController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public CarController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Add([FromBody] Car car)
        {

            if (ModelState.IsValid)
            {

                // Генерация случайных координат центра Воронежа
                Random random = new Random();
                double randomLat = 51.655 + random.NextDouble() * (51.670 - 51.655);
                double randomLon = 39.180 + random.NextDouble() * (39.210 - 39.180);

                car.Latitude = randomLat;
                car.Longitude = randomLon;

                _context.Cars.Add(car);
                _context.SaveChanges();
                return Ok();
            }

            return BadRequest("Неверные данные");
        }

        [HttpGet]
        public IActionResult GetCarCoordinates()
        {
            var cars = _context.Cars.Select(c => new
            {
                lat = c.Latitude,
                lon = c.Longitude,
                title = c.Brand + " " + c.Model,
                imageUrl = c.ImageUrl
            }).ToList();

            return Json(cars);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var car = await _context.Cars
                .Include(c => c.MaintenanceRecords) // Загружаем историю ТО
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (car == null)
                return NotFound();

            var viewModel = new CarEditViewModel
            {
                Car = car,
                NewMaintenanceRecord = new MaintenanceRecord { CarId = id, MaintenanceDate = DateTime.Today }
                
            };

            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax)
                return PartialView("Edit", viewModel);

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Edit([FromForm] CarEditViewModel model)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            
            // Исключаем поля, которые не нужны для валидации
            ModelState.Remove("Car.MaintenanceRecords");
            
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState)
                {
                    System.Diagnostics.Debug.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                
                if (isAjax)
                    return Json(new { success = false, errors = ModelState });

                return View("Edit", model);
            }

            try
            {
                var carFromDb = await _context.Cars.FindAsync(model.Car.Id);
                if (carFromDb == null)
                {
                    if (isAjax)
                        return Json(new { success = false, message = "Автомобиль не найден" });
                    return NotFound();
                }

                // Обновляем поля автомобиля
                carFromDb.Brand = model.Car.Brand;
                carFromDb.Model = model.Car.Model;
                carFromDb.Year = model.Car.Year;
                carFromDb.Price = model.Car.Price;
                carFromDb.Description = model.Car.Description;
                carFromDb.ImageUrl = model.Car.ImageUrl;
                carFromDb.TransmissionType = model.Car.TransmissionType;
                carFromDb.FuelType = model.Car.FuelType;
                carFromDb.Category = model.Car.Category;
                carFromDb.IsAvailable = model.Car.IsAvailable;
                carFromDb.AutoNumber = model.Car.AutoNumber;
                carFromDb.VinNumber = model.Car.VinNumber;
                carFromDb.EngineNumber = model.Car.EngineNumber;
                carFromDb.HorsePower = model.Car.HorsePower;
                carFromDb.BodyNumber = model.Car.BodyNumber;
                carFromDb.Color = model.Car.Color;
                
                if (model.Car.Latitude != 0) carFromDb.Latitude = model.Car.Latitude;
                if (model.Car.Longitude != 0) carFromDb.Longitude = model.Car.Longitude;

                
                if (model.NewMaintenanceRecord?.MaintenanceDate != null && 
                    !string.IsNullOrEmpty(model.NewMaintenanceRecord?.Description))
                {
                    var newMaintenanceRecord = new MaintenanceRecord
                    {
                        CarId = model.Car.Id,
                        MaintenanceDate = model.NewMaintenanceRecord.MaintenanceDate,
                        Description = model.NewMaintenanceRecord.Description,
                        CreatedAt = DateTime.Now 
                    };
                    
                    _context.MaintenanceRecords.Add(newMaintenanceRecord);
                }

                await _context.SaveChangesAsync();

                if (isAjax)
                    return Json(new { success = true, message = "🚗 Автомобиль успешно обновлен!" });

                return RedirectToAction("Auto", "Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения: {ex.Message}");
                
                if (isAjax)
                    return Json(new { success = false, message = "❌ Ошибка при сохранении данных" });
                
                ModelState.AddModelError("", "Ошибка при сохранении данных");
                return View("Edit", model);
            }
        }


    }
}

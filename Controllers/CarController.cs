using Microsoft.AspNetCore.Mvc;
using RentAutoWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;


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


    }
}

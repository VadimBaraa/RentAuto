using Microsoft.AspNetCore.Mvc;
using RentAutoWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace RentAutoWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context; // Явно указываем RentAutoWeb.Models.DbContext

        public HomeController(AppDbContext context) // Явно указываем RentAutoWeb.Models.DbContext
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cars = _context.Cars.ToList(); // Получаем список всех автомобилей
            return View(cars);
        }

        public IActionResult Auto()
        {
            return View(); // Возвращает представление "Auto.cshtml" из папки "Home"
        }
        public IActionResult About()
        {
            return View(); // Возвращает представление "About.cshtml" из папки "Home"
        }

        [HttpPost]
        public IActionResult Login(string login, string password)
        {
            // Тут будет логика обработки данных формы
            return View();
        }
    }
}

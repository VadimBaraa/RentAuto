using Microsoft.AspNetCore.Mvc;
using RentAutoWeb.Services;



namespace RentAutoWeb.Controllers
{ 
    public class WeatherController : Controller
    {
        private readonly WeatherService _weatherService;

        public WeatherController(WeatherService weatherService)
        {
            _weatherService = weatherService;
        }

        public async Task<IActionResult> Recommend(double lat = 51.655, double lon = 39.180)
        {
            var weather = await _weatherService.GetCurrentWeatherAsync(lat, lon);
            string recommendation;

            if (weather == null)
            {
                recommendation = "Не удалось получить погоду. Показываем все доступные автомобили.";
            }
            else
            {
                var temp = weather.main.temp;
                var condition = weather.weather.FirstOrDefault()?.main;

                // Пример простой логики
                if (condition == "Rain" || temp < 0)
                    recommendation = "Сегодня дождь или холод — рекомендуем внедорожник или авто с полным приводом.";
                else if (condition == "Clear" && temp > 20)
                    recommendation = "Отличная погода — возможно, вам подойдет кабриолет или электрокар.";
                else
                    recommendation = "Погода нормальная — выбирайте любой удобный вариант.";
            }

            ViewBag.Recommendation = recommendation;
            return View();
        }
    }

}


using RentAutoWeb.Models;


namespace RentAutoWeb.Services
{
    public class GenerateRandomCoordinates
    {
        private static Random _random = new Random();

        public void Generate(Car car)
        {
            // Центр Воронежа: 51.6608, 39.2003
            car.Latitude = 51.6608 + (_random.NextDouble() - 0.5) * 0.02;
            car.Longitude = 39.2003 + (_random.NextDouble() - 0.5) * 0.02;
        }
    }
}
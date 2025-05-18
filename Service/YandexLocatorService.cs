using System.Text;
using System.Text.Json;

namespace RentAutoWeb.Services
{
    public class YandexLocatorService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public YandexLocatorService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetAddressAsync(double latitude, double longitude)
        {
            try
            {
                string apiKey = _configuration["YandexMaps:ApiKey"];
                string url = $"https://geocode-maps.yandex.ru/1.x/?apikey={apiKey}&format=json&geocode={longitude},{latitude}";

                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                // Здесь нужно распарсить JSON ответ от Яндекса
                // Это упрощенный вариант, вы можете доработать его под свои нужды
                return $"Координаты: {latitude}, {longitude}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка при получении адреса: {ex.Message}");
            }
        }
    }
}

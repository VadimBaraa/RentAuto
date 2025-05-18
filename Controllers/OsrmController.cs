using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class OsrmController : Controller
{
    private readonly ILogger<OsrmController> _logger;

    public OsrmController(ILogger<OsrmController> logger)
    {
        _logger = logger;
    }

    [HttpGet("route")]
    public async Task<IActionResult> GetRoute([FromQuery] double startLat, [FromQuery] double startLon,
                                            [FromQuery] double endLat, [FromQuery] double endLon)
    {
        var coordinates = $"{startLon.ToString(System.Globalization.CultureInfo.InvariantCulture)},{startLat.ToString(System.Globalization.CultureInfo.InvariantCulture)};" +
                        $"{endLon.ToString(System.Globalization.CultureInfo.InvariantCulture)},{endLat.ToString(System.Globalization.CultureInfo.InvariantCulture)}";

        var osrmUrl = $"http://localhost:5000/route/v1/driving/{coordinates}?overview=full&geometries=geojson";

        _logger.LogInformation("OSRM Request URL: {url}", osrmUrl);

        using var client = new HttpClient();
        var response = await client.GetAsync(osrmUrl);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("OSRM Error: {statusCode} - {errorContent}", response.StatusCode, errorContent);
            return StatusCode((int)response.StatusCode, errorContent);
        }

        var json = await response.Content.ReadAsStringAsync();
        return Content(json, "application/json");
    }

}

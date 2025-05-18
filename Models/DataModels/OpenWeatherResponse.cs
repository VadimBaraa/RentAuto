public class OpenWeatherResponse
{
    public Main? main { get; set; }
    public Weather[]? weather { get; set; }
    public Wind? wind { get; set; }
}

public class Main
{
    public float temp { get; set; }
}

public class Weather
{
    public string? main { get; set; }  // Например: "Rain", "Clear"
    public string? description { get; set; }
    public string? icon { get; set; }
}

public class Wind
{
    public float speed { get; set; }
}

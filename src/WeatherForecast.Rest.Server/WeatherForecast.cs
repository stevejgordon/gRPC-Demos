using System;

namespace WeatherForecast.Rest.Server
{
    public class WeatherForecast
    {
        public DateTime DateTime { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string Summary { get; set; }
    }
}

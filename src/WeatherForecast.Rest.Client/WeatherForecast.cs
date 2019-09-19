using System;

namespace WeatherForecast.Rest.Client
{
    public class WeatherForecast
    {
        public DateTime DateTime { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF { get; set; }
        public string Summary { get; set; }
    }
}

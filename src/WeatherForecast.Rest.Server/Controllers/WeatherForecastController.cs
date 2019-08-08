using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace WeatherForecast.Rest.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = 
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        [HttpGet]
        public WeatherForecasts Get()
        {
            var rng = new Random();
            var forecasts = Enumerable.Range(1, 100).Select(index => new WeatherForecast
            {
                DateTimeStamp = DateTimeOffset.UtcNow.AddDays(index).ToUnixTimeSeconds(),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            return new WeatherForecasts{ Forecasts = forecasts };
        }
    }
}

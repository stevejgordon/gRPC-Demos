using System;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task<WeatherForecasts> Get()
        {
            var rng = new Random();
            var forecasts = Enumerable.Range(1, 100).Select(index => new WeatherForecast
            {
                DateTime = DateTime.UtcNow.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            await Task.Delay(2000); // Gotta look busy

            return new WeatherForecasts{ Forecasts = forecasts };
        }
    }
}

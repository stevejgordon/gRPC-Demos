using System;
using System.Linq;
using System.Threading.Tasks;
using WeatherForecast.Grpc.Shared;

namespace WeatherForecast.Grpc.Protobuf.Server.Services
{
    public class WeatherService : IWeatherForecasts
    {
        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        public async ValueTask<WeatherResult> GetWeatherAsync()
        {
            var rng = new Random();
            var now = DateTime.UtcNow;

            var forecasts = Enumerable.Range(1, 100).Select(index => new WeatherData
            {
                DateTime = now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
                .ToArray();

            await Task.Delay(2000); // Gotta look busy

            return new WeatherResult { Forecasts = forecasts };
        }
    }
}

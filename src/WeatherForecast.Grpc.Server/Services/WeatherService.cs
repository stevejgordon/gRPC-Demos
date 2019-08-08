using System;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using WeatherForecasting;

namespace WeatherForecast.Grpc.Server.Services
{
    public class WeatherService : WeatherForecasts.WeatherForecastsBase
    {
        private readonly ILogger<WeatherService> _logger;

        private static readonly string[] Summaries =
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public WeatherService(ILogger<WeatherService> logger)
        {
            _logger = logger;
        }

        public override async Task<WeatherReply> GetWeather(WeatherRequest request, ServerCallContext context)
        {
            var rng = new Random();

            var forecasts = Enumerable.Range(1, 100).Select(index => new WeatherForecasting.WeatherForecast
                {
                    DateTimeStamp = DateTimeOffset.UtcNow.AddDays(index).ToUnixTimeSeconds(),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
                .ToArray();

            await Task.Delay(2000); // Gotta look busy

            return new WeatherReply
            {
                Forecasts = { forecasts }
            };
        }

        public override async Task GetWeatherStream(WeatherRequest request, IServerStreamWriter<WeatherForecasting.WeatherForecast> responseStream, ServerCallContext context)
        {
            var rng = new Random();

            var i = 0;

            while (!context.CancellationToken.IsCancellationRequested || i < 100)
            {
                var forecast = new WeatherForecasting.WeatherForecast
                {
                    DateTimeStamp = DateTimeOffset.UtcNow.AddDays(i++).ToUnixTimeSeconds(),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                };

                await responseStream.WriteAsync(forecast);
                
                await Task.Delay(500); // Gotta look busy
            }

            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("The client cancelled their request");
            }
        }
    }
}

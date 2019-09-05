using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using WeatherForecast.Grpc.Proto;

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
            var now = DateTimeOffset.UtcNow;

            var forecasts = Enumerable.Range(1, 100).Select(index => new WeatherData
            {
                DateTimeStamp = now.AddDays(index).ToUnixTimeSeconds(),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
                .ToArray();

            await Task.Delay(2000); // Gotta look busy

            return new WeatherReply
            {
                WeatherData = { forecasts }
            };
        }

        public override async Task GetWeatherStream(WeatherRequest request, IServerStreamWriter<WeatherData> responseStream, ServerCallContext context)
        {
            var rng = new Random();
            var now = DateTimeOffset.UtcNow;

            var i = 0;

            while (!context.CancellationToken.IsCancellationRequested && i < 20)
            {
                var forecast = new WeatherData
                {
                    DateTimeStamp = now.AddDays(i++).ToUnixTimeSeconds(),
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

        public override async Task ClientStreamWeather(IAsyncStreamReader<TownWeatherRequest> requestStream, IServerStreamWriter<TownWeatherForecast> responseStream,
            ServerCallContext context)
        {
            var rng = new Random();
            var now = DateTimeOffset.UtcNow;

            var channel = Channel.CreateUnbounded<TownWeatherForecast>();

            _ = Task.Run(async () =>
            {
                await foreach (var forecast in channel.Reader.ReadAllAsync())
                {
                    //Console.WriteLine($"Sending : {town} {i} = {forecast.DateTimeStamp} | {forecast.TemperatureC} | {forecast.Summary} ");

                    await responseStream.WriteAsync(forecast);
                }
            });

            var getWeatherTasks = new List<Task>();

            try
            {
                while (await requestStream.MoveNext(context.CancellationToken))
                {
                    _logger.LogInformation($"Getting weather for {requestStream.Current.TownName}");

                    getWeatherTasks.Add(GetTownWeatherAsync(requestStream.Current.TownName));
                }

                _logger.LogInformation("Client finished streaming");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occurred");
            }

            await Task.WhenAll(getWeatherTasks); // wait for all responses to be written to the channel

            channel.Writer.TryComplete();

            await channel.Reader.Completion; //  wait for all responses to be sent from the channel

            _logger.LogInformation("Completed response streaming");

            async Task GetTownWeatherAsync(string town)
            {
                for (var i = 0; i < 10; i++)
                {
                    var forecast = new WeatherData
                    {
                        DateTimeStamp = now.AddDays(i).ToUnixTimeSeconds(),
                        TemperatureC = rng.Next(-20, 55),
                        Summary = Summaries[rng.Next(Summaries.Length)]
                    };

                    await Task.Delay(500); // Gotta look busy                    

                    await channel.Writer.WriteAsync(new TownWeatherForecast { TownName = town, WeatherData = forecast });
                }
            }
        }
    }
}

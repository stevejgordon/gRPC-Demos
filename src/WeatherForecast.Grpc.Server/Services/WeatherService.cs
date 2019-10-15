using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using static WeatherForecast.WeatherForecasts;

namespace WeatherForecast.Grpc.Server.Services
{
    public class WeatherService : WeatherForecastsBase
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

        public override async Task<WeatherReply> GetWeather(Empty _, ServerCallContext context)
        {
            var rng = new Random();
            var now = DateTime.UtcNow;

            var forecasts = Enumerable.Range(1, 100).Select(index => new WeatherData
            {
                DateTimeStamp = Timestamp.FromDateTime(now.AddDays(index)),
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

        public override async Task GetWeatherStream(
            Empty _,
            IServerStreamWriter<WeatherData> responseStream,
            ServerCallContext context)
        {
            var rng = new Random();
            var now = DateTime.UtcNow;

            var i = 0;

            while (!context.CancellationToken.IsCancellationRequested && i < 20)
            {
                var forecast = new WeatherData
                {
                    DateTimeStamp = Timestamp.FromDateTime(now.AddDays(i++)),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                };

                _logger.LogInformation("Sending WeatherData response");

                await responseStream.WriteAsync(forecast);

                await Task.Delay(500); // Gotta look busy
            }

            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("The client cancelled their request");
            }
        }

        public override async Task GetTownWeatherStream(
            IAsyncStreamReader<TownWeatherRequest> requestStream,
            IServerStreamWriter<TownWeatherForecast> responseStream,
            ServerCallContext context)
        {
            var rng = new Random();
            var now = DateTime.UtcNow;

            // we'll use a channel here to handle in-process 'messages' concurrently being written to and read from the channel.
            var channel = Channel.CreateUnbounded<TownWeatherForecast>();

            // background task which uses async streams to write each forecast from the channel to the response steam.
            _ = Task.Run(async () =>
            {
                await foreach (var forecast in channel.Reader.ReadAllAsync())
                {
                    await responseStream.WriteAsync(forecast);
                }
            });

            // a list of tasks handling requests concurrently
            var getTownWeatherRequestTasks = new List<Task>();

            try
            {
                // async streams used to process each request from the stream as they are receieved
                await foreach (var request in requestStream.ReadAllAsync())
                {
                    _logger.LogInformation($"Getting weather for {request.TownName}");
                    getTownWeatherRequestTasks.Add(GetTownWeatherAsync(request.TownName)); // start and add the request handling task
                }

                _logger.LogInformation("Client finished streaming");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An exception occurred");
            }

            // wait for all responses to be written to the channel 
            // from the concurrent tasks handling each request
            await Task.WhenAll(getTownWeatherRequestTasks);

            channel.Writer.TryComplete();

            //  wait for all responses to be read from the channel and streamed as responses
            await channel.Reader.Completion;

            _logger.LogInformation("Completed response streaming");

            // a local function which defines a task to handle a town forecast request
            // it produces 10 forecasts for each town, simulating a 0.5s time to gather each forecast
            // multiple instances of this will run concurrently for each recieved request
            async Task GetTownWeatherAsync(string town)
            {
                for (var i = 0; i < 10; i++)
                {
                    var forecast = new WeatherData
                    {
                        DateTimeStamp = Timestamp.FromDateTime(now.AddDays(i)),
                        TemperatureC = rng.Next(-20, 55),
                        Summary = Summaries[rng.Next(Summaries.Length)]
                    };

                    await Task.Delay(500); // Gotta look busy                    

                    // write the forecast to the channel which will be picked up concurrently by the channel reading background task
                    await channel.Writer.WriteAsync(new TownWeatherForecast
                    {
                        TownName = town,
                        WeatherData = forecast
                    });
                }
            }
        }
    }
}

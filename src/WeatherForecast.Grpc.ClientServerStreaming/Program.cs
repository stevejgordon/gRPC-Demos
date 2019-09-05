using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using WeatherForecast.Grpc.Proto;

namespace WeatherForecast.Grpc.ClientServerStreaming
{
    internal class Program
    {
        private static readonly string[] Towns =
        {
            "London", "Brighton", "Eastbourne", "Seaford", "Hastings", "Oxford", "Cambridge"
        };

        private static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5005");
            var client = new WeatherForecasts.WeatherForecastsClient(channel);

            using var townForecast = client.ClientStreamWeather();

            var responseProcessing = Task.Run(async () =>
            {
                try
                {
                    await foreach (var forecast in townForecast.ResponseStream.ReadAllAsync())
                    {
                        var date = DateTimeOffset.FromUnixTimeSeconds(forecast.WeatherData.DateTimeStamp);

                        Console.WriteLine($"{forecast.TownName} = {date:s} | {forecast.WeatherData.Summary} | {forecast.WeatherData.TemperatureC} C");
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    Console.WriteLine("Stream cancelled.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading response: " + ex);
                }
            });

            foreach (var town in Towns)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Requesting forecast for {town}...");
                Console.ResetColor();

                await townForecast.RequestStream.WriteAsync(new TownWeatherRequest{ TownName = town });
                               
                await Task.Delay(2500); // simulate delay getting next item
            }

            Console.WriteLine("Completing request stream");
            await townForecast.RequestStream.CompleteAsync();
            Console.WriteLine("Request stream completed");

            await responseProcessing;

            Console.WriteLine("Read all responses");
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}

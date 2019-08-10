using System;
using System.Net.Http;
using System.Threading;
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
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5005")
            };

            var client = GrpcClient.Create<WeatherForecasts.WeatherForecastsClient>(httpClient);

            using var townForecast = client.ClientStreamWeather();

            _ = Task.Run(async () =>
            {
                try
                {
                    while (await townForecast.ResponseStream.MoveNext(CancellationToken.None))
                    {
                        var response = townForecast.ResponseStream.Current;

                        var date = DateTimeOffset.FromUnixTimeSeconds(response.WeatherData.DateTimeStamp);

                        Console.WriteLine($"{response.TownName} = {date:s} | {response.WeatherData.Summary} | {response.WeatherData.TemperatureC} C");
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
                
            Console.WriteLine("Disconnecting");
            await townForecast.RequestStream.CompleteAsync();

            Console.WriteLine("Disconnected. Press any key to exit.");
            Console.ReadKey();
        }
    }
}

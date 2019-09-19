using System;
using System.Net.Http;
using System.Threading.Tasks;
using ProtoBuf.Grpc.Client;
using WeatherForecast.Grpc.Shared;

namespace WeatherForecast.Grpc.Protobuf.Client
{
    internal class Program
    {
        private static async Task Main()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5012")
            };

            var client = httpClient.CreateGrpcService<IWeatherForecasts>();

            var response = await client.GetWeatherAsync();

            foreach (var forecast in response.Forecasts)
            {
                Console.WriteLine($"{forecast.DateTime:s} | {forecast.Summary} | {forecast.TemperatureC} C");
            }

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}

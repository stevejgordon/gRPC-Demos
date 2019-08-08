using System;
using System.Net.Http;
using System.Threading.Tasks;
using Grpc.Net.Client;
using WeatherForecasting;

namespace WeatherForecast.Grpc.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5005")
            };

            var client = GrpcClient.Create<WeatherForecasts.WeatherForecastsClient>(httpClient);

            var response = await client.GetWeatherAsync(new WeatherRequest());

            foreach (var forecast in response.Forecasts)
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(forecast.DateTimeStamp);

                Console.WriteLine($"{date:s} | {forecast.Summary} | {forecast.TemperatureC} C");
            }

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}

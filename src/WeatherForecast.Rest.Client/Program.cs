using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeatherForecast.Rest.Client
{
    internal class Program
    {
        private static async Task Main()
        {
            var httpClient = new HttpClient { BaseAddress = new Uri("https://localhost:5001") }; // demoware!

            var response = await httpClient.GetAsync("weatherforecast");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(); // demoware!
                var forecasts = JsonSerializer.Deserialize<WeatherForecasts>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                foreach (var forecast in forecasts.Forecasts)
                {
                    var date = DateTimeOffset.FromUnixTimeSeconds(forecast.DateTimeStamp);

                    Console.WriteLine($"{date:s} | {forecast.Summary} | {forecast.TemperatureC} C");
                }
            }

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}

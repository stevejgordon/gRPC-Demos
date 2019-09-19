using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using WeatherForecast.Grpc.Proto;

namespace WeatherForecast.Grpc.Client
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5005");
            var client = new WeatherForecasts.WeatherForecastsClient(channel);

            var reply = await client.GetWeatherAsync(new Empty());

            foreach (var forecast in reply.WeatherData)
            {
                Console.WriteLine($"{forecast.DateTimeStamp.ToDateTime():s} | {forecast.Summary} | {forecast.TemperatureC} C");
            }

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using WeatherForecast.Grpc.Proto;

namespace WeatherForecast.Grpc.StreamingClient
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:5005");
            var client = new WeatherForecasts.WeatherForecastsClient(channel);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            using var replies = client.GetWeatherStream(new WeatherRequest(), cancellationToken: cts.Token);

            try
            {
                await foreach (var weatherData in replies.ResponseStream.ReadAllAsync(cancellationToken: cts.Token))
                {
                    var date = DateTimeOffset.FromUnixTimeSeconds(weatherData.DateTimeStamp);

                    Console.WriteLine($"{date:s} | {weatherData.Summary} | {weatherData.TemperatureC} C");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {               
                Console.WriteLine("Stream cancelled.");
            }
            
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}

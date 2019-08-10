using System;
using System.Net.Http;
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
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5005")
            };

            var client = GrpcClient.Create<WeatherForecasts.WeatherForecastsClient>(httpClient);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            using var replies = client.GetWeatherStream(new WeatherRequest(), cancellationToken: cts.Token);

            try
            {
                while (await replies.ResponseStream.MoveNext(cts.Token))
                {
                    var current = replies.ResponseStream.Current;

                    var date = DateTimeOffset.FromUnixTimeSeconds(current.DateTimeStamp);

                    Console.WriteLine($"{date:s} | {current.Summary} | {current.TemperatureC} C");
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                replies.Dispose();

                Console.WriteLine("Stream cancelled.");
            }
            
            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}

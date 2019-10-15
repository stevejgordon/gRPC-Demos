using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Health.V1;
using Grpc.Net.Client;

namespace WeatherForecast.Grpc.HealthCheckClient
{
    internal class Program
    {
        private static async Task Main()
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5005");

            var healthClient = new Health.HealthClient(channel);

            var health = await healthClient.CheckAsync(new HealthCheckRequest { Service = "Weather" });

            Console.WriteLine($"Health Status: {health.Status}");

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Health.V1;
using Grpc.HealthCheck;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace WeatherForecast.Grpc.Server
{
    public class StatusService : BackgroundService
    {
        private readonly HealthServiceImpl _healthService;
        private readonly HealthCheckService _healthCheckService;

        public StatusService(HealthServiceImpl healthService, HealthCheckService healthCheckService)
        {
            _healthService = healthService;
            _healthCheckService = healthCheckService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var health = await _healthCheckService.CheckHealthAsync(stoppingToken);

                _healthService.SetStatus("Weather",
                    health.Status == HealthStatus.Healthy
                        ? HealthCheckResponse.Types.ServingStatus.Serving
                        : HealthCheckResponse.Types.ServingStatus.NotServing);

                _healthService.SetStatus(string.Empty,
                    health.Status == HealthStatus.Healthy
                        ? HealthCheckResponse.Types.ServingStatus.Serving
                        : HealthCheckResponse.Types.ServingStatus.NotServing);

                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}

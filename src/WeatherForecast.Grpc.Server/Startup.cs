using System.Collections.Generic;
using System.IO.Compression;
using Grpc.HealthCheck;
using Grpc.Net.Compression;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WeatherForecast.Grpc.Server.Services;

namespace WeatherForecast.Grpc.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddGrpc(o =>
            //{
            //    o.ResponseCompressionLevel = CompressionLevel.Optimal;
            //    o.ResponseCompressionAlgorithm = "gzip";
            //});

            services.AddGrpc();

            services.AddHealthChecks();

            services.AddSingleton<HealthServiceImpl>();

            services.AddHostedService<StatusService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // health checks
                endpoints.MapHealthChecks("/health");
                endpoints.MapGrpcService<HealthServiceImpl>();

                endpoints.MapGrpcService<WeatherService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}

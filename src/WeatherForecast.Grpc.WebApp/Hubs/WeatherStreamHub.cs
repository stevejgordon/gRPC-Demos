using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using WeatherForecast.Grpc.Proto;

namespace WeatherForecast.Grpc.WebApp.Hubs
{
    public class WeatherStreamHub : Hub
    {
        private readonly WeatherForecasts.WeatherForecastsClient _client;

        public WeatherStreamHub(WeatherForecasts.WeatherForecastsClient client)
        {
            _client = client;
        }

        public ChannelReader<string> WeatherStream(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<string>();

            _ = WriteItemsAsync(channel.Writer);

            return channel.Reader;

            async Task WriteItemsAsync(ChannelWriter<string> writer)
            {
                using var replies = _client.GetWeatherStream(new WeatherRequest(), cancellationToken: cancellationToken);

                try
                {
                    await foreach(var forecast in replies.ResponseStream.ReadAllAsync())
                    {                  
                        var date = DateTimeOffset.FromUnixTimeSeconds(forecast.DateTimeStamp);
                        await writer.WriteAsync($"{date:s} | {forecast.Summary} | {forecast.TemperatureC} C", cancellationToken);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
                    replies.Dispose();
                    Console.WriteLine("Stream cancelled.");
                }
                catch (Exception ex)
                {
                    writer.TryComplete(ex);
                }

                writer.TryComplete();
            }
        }
    }
}

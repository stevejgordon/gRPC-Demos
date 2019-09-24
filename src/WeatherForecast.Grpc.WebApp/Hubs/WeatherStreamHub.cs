using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.SignalR;
using static WeatherForecast.WeatherForecasts;

namespace WeatherForecast.Grpc.WebApp.Hubs
{
    public class WeatherStreamHub : Hub
    {
        private readonly WeatherForecastsClient _client;

        public WeatherStreamHub(WeatherForecastsClient client)
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
                using var replies = _client.GetWeatherStream(new Empty(), cancellationToken: cancellationToken);

                try
                {
                    await foreach(var forecast in replies.ResponseStream.ReadAllAsync())
                    {                  
                        await writer.WriteAsync($"{forecast.DateTimeStamp.ToDateTime():d} | {forecast.Summary} | {forecast.TemperatureC} C", cancellationToken);
                    }
                }
                catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
                {
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

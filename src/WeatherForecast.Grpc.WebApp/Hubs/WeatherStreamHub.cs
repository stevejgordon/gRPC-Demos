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
                    while (await replies.ResponseStream.MoveNext(cancellationToken))
                    {
                        var current = replies.ResponseStream.Current;

                        var date = DateTimeOffset.FromUnixTimeSeconds(current.DateTimeStamp);

                        await writer.WriteAsync($"{date:s} | {current.Summary} | {current.TemperatureC} C", cancellationToken);
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

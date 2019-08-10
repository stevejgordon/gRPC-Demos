using System.ServiceModel;
using System.Threading.Tasks;

namespace WeatherForecast.Grpc.Shared
{
    [ServiceContract(Name = "WeatherForecasting.Grpc")]
    public interface IWeatherForecasts
    {
        ValueTask<WeatherResult> GetWeatherAsync();
    }
}

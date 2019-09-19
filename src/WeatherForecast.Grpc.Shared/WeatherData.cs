using System;
using System.Runtime.Serialization;

namespace WeatherForecast.Grpc.Shared
{
    [DataContract]
    public class WeatherData
    {
        [DataMember(Order = 1)]
        public DateTime DateTime { get; set; }

        [DataMember(Order = 2)]
        public int TemperatureC { get; set; }

        [DataMember(Order = 3)]
        public int TemperatureF { get; set; }

        [DataMember(Order = 4)]
        public string Summary { get; set; }
    }
}

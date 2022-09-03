namespace PMetrium.Native.Host.Settings
{
    public class ConfigModel
    {
        public Influxdb InfluxDB { get; set; }
        public int WireMockPort { get; set; }
        public string LogLevel { get; set; }
        public TimeSpan AppEventsTimeout { get; set; }
    }


    public class Influxdb
    {
        public string Url { get; set; }
        public string DataBaseName { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
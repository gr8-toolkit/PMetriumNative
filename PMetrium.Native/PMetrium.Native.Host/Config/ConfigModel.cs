namespace PMetrium.Native.Host.Config
{
    public class ConfigModel
    {
        public Influxdb InfluxDB { get; set; }
    }


    public class Influxdb
    {
        public string Url { get; set; }
        public string DataBaseName { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
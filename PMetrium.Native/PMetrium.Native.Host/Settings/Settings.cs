using Microsoft.Extensions.Configuration;

namespace PMetrium.Native.Host.Settings
{
    public static class Settings
    {
        public static ConfigModel Instance;

        static Settings()
        {
            Instance = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT")}.json", true)
                .AddEnvironmentVariables()
                .Build().Get<ConfigModel>()!;
        }
    }
}
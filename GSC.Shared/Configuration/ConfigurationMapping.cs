using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace GSC.Shared.Configuration
{
    public static class ConfigurationMapping
    {
        public static void AddConfig(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<SafeIPAddress>(configuration.GetSection("safeIPAddress"));
            services.Configure<EnvironmentSetting>(configuration.GetSection("EnvironmentSetting"));
        }
    }

    public class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int MinutesToExpiration { get; set; }
    }

    public class EnvironmentSetting
    {
        public bool IsPremise { get; set; }
        public string CentralApi { get; set; }
        public string ClientSqlConnection { get; set; }
    }

    public class SafeIPAddress
    {
        public List<string> IpList { get; set; }
    }
}

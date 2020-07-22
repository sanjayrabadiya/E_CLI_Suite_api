using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Helper
{
    public static class ConfigurationMapping
    {
        public static void AddConfig(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<SafeIPAddress>(configuration.GetSection("safeIPAddress"));
        }
    }

    public class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int MinutesToExpiration { get; set; }
    }

    public class SafeIPAddress
    {
        public List<string> IpList { get; set; }
    }
}

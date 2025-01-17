﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;

namespace GSC.Shared.Configuration
{
    public static class ConfigurationMapping
    {
        public static EnvironmentSetting EnvironmentSetting ;
        public static void AddConfig(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            services.Configure<SafeIPAddress>(configuration.GetSection("safeIPAddress"));
            services.Configure<VideoAPISettings>(configuration.GetSection("VideoAPISettings"));
            services.Configure<EnvironmentSetting>(configuration.GetSection("EnvironmentSetting"));
            var environmentSetting = new EnvironmentSetting();
            configuration.GetSection("EnvironmentSetting").Bind(environmentSetting);
            EnvironmentSetting = environmentSetting;
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
    }

    public class SafeIPAddress
    {
        public List<string> IpList { get; set; }
    }

    public class VideoAPISettings
    {
        public string API_KEY { get; set; }
        public string API_SECRET { get; set; }
    }
}

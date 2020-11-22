using GSC.Shared;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System.IO;

namespace GSC.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LogConfiguration.Configure();

            var webHost = CreateWebHostBuilder(args);

            webHost.Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                //.UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseSetting("detailedErrors", "true")
                .UseIISIntegration()
                .UseStartup<Startup>()
                .CaptureStartupErrors(true)
                .UseSerilog();
        }
    }
}
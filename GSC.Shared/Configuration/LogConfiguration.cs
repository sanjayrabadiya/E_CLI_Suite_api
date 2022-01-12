using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace GSC.Shared.Configuration
{
    public class LogConfiguration
    {
        public static void Configure()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("Config//appsettings.json")
                       .AddEnvironmentVariables()
                       .Build();

            var applicationName = Assembly.GetEntryAssembly()?.GetName().Name;



            Log.Logger = new LoggerConfiguration()
                          .ReadFrom.Configuration(configuration)
                          .Enrich.WithThreadId()
                        .CreateLogger();
        }
    }

    public class LogMiddleware
    {
        private readonly RequestDelegate _next;

        public LogMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var bodyStr = "";
            var user = httpContext.Request.Headers["user"];
            if (!string.IsNullOrEmpty(user))
            {
                var userdata = JsonConvert.DeserializeObject<UserInfo>(user);
                LogContext.PushProperty("UserId", userdata.UserId);
                LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier);
            }
            try
            {
                var req = httpContext.Request;
                req.EnableBuffering();
                using (StreamReader reader = new StreamReader(req.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyStr = await reader.ReadToEndAsync();
                    req.Body.Position = 0;

                }
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleException(httpContext, ex, bodyStr);
            }

        }

        private async Task HandleException(HttpContext httpContext, Exception ex, string bodyStr)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            var message = ParseMessage(ex);
            Log.Error(ex, $"{httpContext.Request.Method} || {httpContext.Request.Path} || {ex.Message}");
            await httpContext.Response.WriteAsync(message);
        }

        private static string ParseMessage(Exception ex)
        {
            var message = ex.InnerException == null
                ? ex.Message
                : ex.InnerException.Message;

            SqlException baseException = null;
            if (ex.Source.Contains("EntityFramework"))
            {
                baseException = ex.GetBaseException() as SqlException;
            }
            else
            {
                baseException = ex as SqlException;
            }

            if (baseException == null)
            {
                return message;
            }

            var errorNo = baseException.Number;

            switch (errorNo)
            {
                case 547: //Reference Error
                    if (baseException.Message.Contains("DELETE statement"))
                    {
                        message = "You cannot delete this record, reference exists for this record.";
                    }
                    else if (baseException.Message.Contains("INSERT statement"))
                    {
                        message = "Reference conflict, cannot Insert.";
                    }
                    else
                    {
                        message = "Reference Error.";
                    }
                    break;

                case 2627:
                case 2601:
                    message = "Record already exists with same data.";
                    break;

                default:
                    break;
            }

            return message;
        }
    }
}

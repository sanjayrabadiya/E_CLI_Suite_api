using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace GSC.Helper
{
    public class LogConfiguration
    {
        public static void Configure()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
                       .AddJsonFile("appsettings.json")
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

        public Task Invoke(HttpContext httpContext)
        {
            var user = httpContext.Request.Headers["user"];
            if (!string.IsNullOrEmpty(user))
            {
                var userdata = JsonConvert.DeserializeObject<UserInfo>(user);
                LogContext.PushProperty("UserId", userdata.UserId);
                LogContext.PushProperty("CorrelationId", httpContext.TraceIdentifier);
            }
            try
            {
                return _next(httpContext);
            }
            catch (Exception ex)
            {
                return HandleException(httpContext, ex);
            }
           
        }

        private Task HandleException(HttpContext httpContext, Exception ex)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        var message = ParseMessage(ex);
            Log.Error(ex, "HandleException");
            return httpContext.Response.WriteAsync(message);
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

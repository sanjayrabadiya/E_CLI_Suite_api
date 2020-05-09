using GSC.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Net;
using System.Threading.Tasks;

namespace GSC.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILoggerFactory loggerFactory;

        public ExceptionMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            this.next = next;
            this.loggerFactory = loggerFactory;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var message = ParseMessage(ex);

            var logger = loggerFactory.CreateLogger("Global exception logger");
            logger.LogError(httpContext.Response.StatusCode, ex, message);

            return httpContext.Response.WriteAsync(message);
        }

        private static string ParseMessage(Exception ex)
        {
            var message = ex.GetFullMessage();

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
                    message = ex.GetFullMessage();
                    break;
            }

            return message;
        }
    }
}

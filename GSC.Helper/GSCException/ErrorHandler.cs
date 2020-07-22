
using System;
using System.Net;
using GSC.Helper.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Serilog;

namespace GSC.Helper.GSCException
{
    public static class ErrorHandler
    {
        public static Action<IApplicationBuilder> HttpExceptionHandling(IWebHostEnvironment env)
        {
            return options =>
            {
                options.Run(
                async context =>
                {

                    context.Response.ContentType = context.Request.ContentType;
                    var ex = context.Features.Get<IExceptionHandlerFeature>();
                    if (ex.Error.GetType() == typeof(ValidModelException))
                    {
                        var modelStateDic = new ModelStateDictionary();
                        modelStateDic.AddModelError("Message", ex.Error.Message);
                        context.Response.StatusCode = 422;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                        Validation.UnprocessableEntityObjectResult(modelStateDic))).ConfigureAwait(false);
                    }
                    else
                    {
                        if (ex.Error.GetType() == typeof(ModelNotFoundException))
                            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        else
                        {
                            Log.Error(ex.Error, "Exception");
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        }
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            message = env.IsDevelopment() ? ex.Error.ToString() : "Error Processing your request, please retry after sometime"
                        })).ConfigureAwait(false);
                    }
                });
            };
        }

    }

}

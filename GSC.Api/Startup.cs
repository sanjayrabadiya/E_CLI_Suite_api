using System.IO;
using System.Reflection;
using AutoMapper;
using GSC.Api.Helpers;
using GSC.Domain.Context;
using GSC.Shared.Configuration;
using GSC.Shared.Filter;
using GSC.Shared.GSCException;
using GSC.Shared.JWTAuth;
using GSC.Shared.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using GSC.Api.Hosted;
using Quartz;
using GSC.Api.QuartzJob;
using System;

namespace GSC.Api
{
    public class Startup
    {
        private static IConfigurationRoot _configuration;

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables()
                .AddJsonFile("Config//ipAddress.json", false, true)
                .AddJsonFile("Config//appSettings.json", false, true);
            _configuration = builder.Build();
        }



        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuth(_configuration);
            services.AddConfig(_configuration);

            services.AddDependencyInjection<IGSCContext>(_configuration);

            services.AddAutoMapper(Assembly.GetAssembly(typeof(AutoMapperConfiguration)));
            services.AddScoped<AllowedSafeCallerFilter>();

            services.AddControllers(options =>
            {
                options.EnableEndpointRouting = false;
                options.Filters.Add(typeof(ValidateModelAttribute));
                options.Filters.Add(typeof(TransactionFilter));
            }).AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "GSC API", Version = "v3.1" });
            });
            services.AddHttpContextAccessor();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VlhhQlJCfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX5Wd0JjWH1WcX1WTmhV");

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseExceptionHandler(ErrorHandler.HttpExceptionHandling(env));
            app.UseAuthentication();
            app.UseMiddleware<LogMiddleware>();
            app.UseCors(builder =>
            {
                builder.WithOrigins(new[] { "http://localhost:4100", "http://localhost:4200", "http://localhost:63980", "https://dev2.clinvigilant.com", "https://demo1.clinvigilant.com", "https://sandbox.clinvigilant.com",
                "https://dev.clinvigilant.com", "https://eclinical.clinvigilant.com","https://devapi.clinvigilant.com/"})
                .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            });
            app.UseStaticFiles();

            app.UseSwagger();

            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "GSC API v3.1"); });

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(e =>
            {
                e.MapControllers();
            });
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mgo+DSMBMAY9C3t2VlhhQlJCfV5AQmBIYVp/TGpJfl96cVxMZVVBJAtUQF1hTX5Wd0JjWH1WcX1WTmhV");
        }


    }
}
using System.IO;
using System.Reflection;
using AutoMapper;
using GSC.Api.Helpers;
using GSC.Api.Hubs;
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
            services.AddHostedService<VolunteerUnblockService>();
            //services.AddSignalR();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NjI3MzgxQDMxMzkyZTMxMmUzMEtnSHlnTG9ZeG82SHlEV25hR1BhSXJkQ1VaN21sTzJEdlMrL1RPbkZnTDg9");

            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            app.UseExceptionHandler(ErrorHandler.HttpExceptionHandling(env));
            app.UseAuthentication();
            app.UseMiddleware<LogMiddleware>();
           // app.UseCors("AllowCorsPolicy");
            app.UseCors(builder =>
            {
                builder.WithOrigins(new[] { "http://localhost:4100", "http://localhost:4200", "http://localhost:63980", "https://dev2.clinvigilant.com", "https://demo1.clinvigilant.com", "https://sandbox.clinvigilant.com",
                "https://dev.clinvigilant.com", "https://eclinical.clinvigilant.com"})
                .AllowAnyHeader().AllowAnyMethod().AllowCredentials();
            });
            app.UseStaticFiles();
            var doc = _configuration["DocPath:DocDir"];

            if (!string.IsNullOrEmpty(doc))
            {
                if (!Directory.Exists(doc))
                    Directory.CreateDirectory(doc);
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(Directory.GetCurrentDirectory(), "TempDoc")),
                    RequestPath = "/static"
                });
            }

            app.UseSwagger();

            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "GSC API v3.1"); });

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(e => 
            {
                e.MapControllers();
               // e.MapHub<MessageHub>("/MessageHub");
            });
            //app.UseSpa(spa => { spa.Options.SourcePath = "wwwroot"; });
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzgxNjc2QDMxMzgyZTM0MmUzMG9ETm5BR0xPZzdMbjN0dTcwbjJhUmw2SUtqNUxYaEc4WFNrNXcwUzZvdEk9");
        }


    }
}
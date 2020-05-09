using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using AutoMapper;
using GSC.Api.Helpers;
using GSC.Api.Middleware;
using GSC.Data.Dto.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;

namespace GSC.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Get JWT Token Settings from JwtSettings.json file
            JwtSettings settings;
            settings = GetJwtSettings();
            services.AddHttpContextAccessor();
            services.AddAuth(settings);
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });

                // Swagger 2.+ support
                var security = new Dictionary<string, IEnumerable<string>>
                {
                    {"Bearer", new string[] { }}
                };

                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Description =
                        "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(security);
            });
            // Create singleton of JwtSettings
            services.AddSingleton(settings);

            services.AddDbContextPool<GscContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("dbConnectionString"));
            });
            services.AddDependencyInjection(Configuration);

            services.AddCors(options =>
            {
                options.AddPolicy("ExposeResponseHeaders",
                    builder =>
                    {
                        builder.WithOrigins("http://localhost:4200")
                            .WithExposedHeaders("X-Pagination")
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });
            services.AddAutoMapper(System.Reflection.Assembly.GetAssembly(typeof(AutoMapperConfiguration)));
            services.AddMvc()
                .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver =
                            new CamelCasePropertyNamesContractResolver();
                        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    }
                );
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseCustomException();
            app.UseCors("ExposeResponseHeaders");
            app.UseStaticFiles();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty;
            });


            app.UseAuthentication();
            //Mapper.Initialize(x => { x.AddProfile<AutoMapperConfiguration>(); });

            //Mapper.Configuration.AssertConfigurationIsValid();
            app.UseMvc();

            app.UseSignalR(routes => { routes.MapHub<Notification>("/notify"); });
        }

        public JwtSettings GetJwtSettings()
        {
            var settings = new JwtSettings();

            settings.Key = Configuration["JwtSettings:key"];
            settings.Audience = Configuration["JwtSettings:audience"];
            settings.Issuer = Configuration["JwtSettings:issuer"];
            settings.MinutesToExpiration =
                Convert.ToInt32(
                    Configuration["JwtSettings:minutesToExpiration"]);

            return settings;
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
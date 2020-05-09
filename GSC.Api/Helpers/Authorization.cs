using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSC.Data.Dto.UserMgt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace GSC.Api.Helpers
{
    public static class Authorization
    {
        public static void AddAuth(this IServiceCollection services, JwtSettings configuration)
        {
            services.AddCors(o => o.AddPolicy("AllowCorsPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader().Build();
            }));
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(jwtBearerOptions =>
                {
                    jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ClockSkew = new TimeSpan(0),
                        ValidateActor = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration.Issuer,
                        ValidAudience = configuration.Audience,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(configuration.Key))
                    };
                    jwtBearerOptions.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            if (context.SecurityToken is JwtSecurityToken accessToken)
                            {
                                var userInfo = accessToken.Claims.FirstOrDefault(a => a.Type == "gsc_user_token")
                                    ?.Value;
                                context.HttpContext.Request.Headers.Add("user", userInfo);
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
        }
    }
}
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace GSC.Shared.JWTAuth
{
    public static class Authorization
    {
        public static void AddAuth(this IServiceCollection services, IConfigurationRoot configuration)
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
                        ValidateActor = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = configuration["JwtSettings:Issuer"],
                        ValidAudience = configuration["JwtSettings:Audience"],
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(configuration["JwtSettings:key"]))
                    };
                    jwtBearerOptions.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            if (context.SecurityToken is JwtSecurityToken accessToken)
                            {
                                var userInfo = accessToken.Claims.Where(a => a.Type == "gsc_user_token").FirstOrDefault()?.Value;
                                if (userInfo == null)
                                    userInfo = accessToken.Claims.Where(a => a.Type == ClaimTypes.GivenName).FirstOrDefault()?.Value;

                                context.HttpContext.Request.Headers.Add("user", userInfo);
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

        }
    }
}

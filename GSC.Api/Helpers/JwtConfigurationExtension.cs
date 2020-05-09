using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GSC.Data.Dto.UserMgt;

namespace GSC.Api.Helpers
{
    public static class JwtAuthenticationConfigurationExtension
    {
        public static void AddJwtAutheticationConfiguration(this IServiceCollection services, JwtSettings settings)
        {
            // Register Jwt as the Authentication service
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "JwtBearer";
                options.DefaultChallengeScheme = "JwtBearer";
            })
            .AddJwtBearer("JwtBearer", jwtBearerOptions =>
            {
                jwtBearerOptions.TokenValidationParameters =
              new TokenValidationParameters
              {
                  ValidateIssuerSigningKey = true,
                  IssuerSigningKey = new SymmetricSecurityKey(
                  Encoding.UTF8.GetBytes(settings.Key)),
                  ValidateIssuer = true,
                  ValidIssuer = settings.Issuer,

                  ValidateAudience = true,
                  ValidAudience = settings.Audience,
                  ValidateLifetime = true,
                  ClockSkew = TimeSpan.FromMinutes(
                         settings.MinutesToExpiration)
              };
                jwtBearerOptions.Events = new JwtBearerEvents
                {
                    OnTokenValidated = context =>
                    {
                        if (context.SecurityToken is JwtSecurityToken accessToken)
                        {

                            var userName = Convert.ToInt32(accessToken.Claims.FirstOrDefault(a => a.Type == JwtRegisteredClaimNames.Sub)?.Value);
                            var companyId = Convert.ToInt32(accessToken.Claims.FirstOrDefault(a => a.Type == JwtRegisteredClaimNames.Acr)?.Value);
                            var roleId = Convert.ToInt32(accessToken.Claims.FirstOrDefault(a => a.Type == JwtRegisteredClaimNames.Typ)?.Value);
                            var address = context.HttpContext.Connection.RemoteIpAddress.ToString();
                        }

                        return Task.CompletedTask;
                    }
                };

            });

            services.AddAuthorization(cfg =>
            {
                // NOTE: The claim type and value are case-sensitive
                cfg.AddPolicy("CanAccessProducts", p => p.RequireClaim("CanAccessProducts", "true"));
            });

        }
    }
}

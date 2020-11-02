using GSC.Centeral.Context;
using GSC.Centeral.GenericRespository;
using GSC.Centeral.Models;
using GSC.Centeral.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GSC.Respository.CenteralAuth
{
    public class CenteralRepository : GenericCenteralRespository<Users, CenteralContext>, ICenteralRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;    
        private readonly IOptions<JwtSettings> _settings;
        private readonly IRefreshTokenCenteralRepository _refreshTokenCenteralRepository;
        private readonly IUnitOfWorkCenteral<CenteralContext> _uow;

        public CenteralRepository(IUnitOfWorkCenteral<CenteralContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IOptions<JwtSettings> settings, IRefreshTokenCenteralRepository refreshTokenCenteralRepository)
            : base(uow, jwtTokenAccesser)
        {          
            _jwtTokenAccesser = jwtTokenAccesser;
            _settings = settings;
            _refreshTokenCenteralRepository = refreshTokenCenteralRepository;
            _uow = uow;
        }
        public Users CheckValidUser(string userName)
        {
            return All.Where(x => x.UserName == userName && x.DeletedDate == null).SingleOrDefault();
        }

        public void UpdateRefreshToken(int userid, string refreshToken)
        {           
            var rereshToken = _refreshTokenCenteralRepository.FindBy(t => t.UserId == userid).FirstOrDefault();

            if (rereshToken == null)
            {
                rereshToken = new RefreshToken();
                rereshToken.UserId = userid;
                rereshToken.Token = refreshToken;
                rereshToken.ExpiredOn = DateTime.UtcNow.AddDays(1);
                _refreshTokenCenteralRepository.Add(rereshToken);
            }
            else
            {
                rereshToken.Token = refreshToken;
                rereshToken.ExpiredOn = DateTime.UtcNow.AddDays(1);
                _refreshTokenCenteralRepository.Update(rereshToken);
            }
            _uow.Save();
        }

        public RefreshTokenDto Refresh(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);

            var login = Context.RefreshToken.FirstOrDefault(t =>
                t.Token == refreshToken && t.ExpiredOn > DateTime.UtcNow);

            if (login == null) throw new SecurityTokenException("Refresh token not found or has been expired.");

            return new RefreshTokenDto
            {
                AccessToken = GenerateAccessToken(principal.Claims),
                ExpiredAfter = DateTime.UtcNow.AddMinutes(_settings.Value.MinutesToExpiration),
                RefreshToken = refreshToken
            };
        }


        public string GenerateAccessToken(IEnumerable<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Value.Key));

            var jwtToken = new JwtSecurityToken(_settings.Value.Issuer,
                _settings.Value.Audience,
                claims,
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(_settings.Value.MinutesToExpiration),
                new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(jwtToken);
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience =
                    false, //you might want to validate the audience and issuer depending on your use case
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Value.Key)),
                ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        public string DuplicateUserName(Users objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.UserName == objSave.UserName && x.DeletedDate == null))
                return "Duplicate User Name : " + objSave.UserName;

            if (All.Any(x => x.Id != objSave.Id && x.Email == objSave.Email && x.DeletedDate == null))
                return "Duplicate EMail : " + objSave.Email;

            return "";
        }


        public int Save(Users objSave) {            
            Context.Users.Add(objSave);
            _uow.Save();
            return objSave.Id;
        }
    }
}

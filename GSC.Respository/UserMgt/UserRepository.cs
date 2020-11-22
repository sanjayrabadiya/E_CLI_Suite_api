using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper.Configuration;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Shared;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace GSC.Respository.UserMgt
{
    public class UserRepository : GenericRespository<User>, IUserRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly ILoginPreferenceRepository _loginPreferenceRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        private readonly IUserPasswordRepository _userPasswordRepository;
        private readonly IOptions<JwtSettings> _settings;       
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IAPICall _centeralApi;
        private readonly IGSCContext _context;
        public UserRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            ILoginPreferenceRepository loginPreferenceRepository,
            IUserLoginReportRespository userLoginReportRepository,
            IUserPasswordRepository userPasswordRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IOptions<JwtSettings> settings,
            Microsoft.Extensions.Configuration.IConfiguration configuration, IAPICall centeralApi)
            : base(context)
        {
            _loginPreferenceRepository = loginPreferenceRepository;
            _userLoginReportRepository = userLoginReportRepository;
            _userPasswordRepository = userPasswordRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _settings = settings;
            _refreshTokenRepository = refreshTokenRepository;           
            _configuration = configuration;
            _centeralApi = centeralApi;
            _context = context;
        }

        public List<UserDto> GetUsers(bool isDeleted)
        {
            return All.Where(x =>

              isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).Select(t => new UserDto
            {
                FirstName = t.FirstName,
                MiddleName = t.MiddleName,
                LastName = t.LastName,
                Email = t.Email,
                ProfilePic = t.ProfilePic,
                Id = t.Id,
                UserName = t.UserName,
                IsLocked = t.IsLocked,
                IsDeleted = t.DeletedDate != null,
                Role = string.Join(", ",
                    t.UserRoles.Where(x => x.DeletedDate == null).Select(s => s.SecurityRole.RoleName).ToList())
            }).OrderByDescending(x => x.Id).ToList();
        }

        public User ValidateUser(string userName, string password)
        {
            if (Convert.ToBoolean(_configuration["IsCloud"]))
                return ValidateCenteral(userName, password);
            else
                return validateClient(userName, password);
        }


        private User ValidateCenteral(string userName, string password)
        {
            var user = All.Where(x =>
              (x.UserName == userName || x.Email == userName)
              && x.DeletedDate == null).FirstOrDefault();
            if (user == null)
            {
                _userLoginReportRepository.SaveLog("Invalid User Name", null, userName);
                return null;
            }
            var passDto = new ValidatepasswordDto();
            passDto.UserID = user.Id;
            passDto.Password = password;
            string passstring = JsonConvert.DeserializeObject(_centeralApi.Post(passDto,$"{_configuration["EndPointURL"]}/User/VaidatePassword")).ToString();
            if (!string.IsNullOrEmpty(passstring))
            {
                user.FailedLoginAttempts++;
                var result = _loginPreferenceRepository.FindBy(x => x.CompanyId == user.CompanyId).FirstOrDefault();
                if (result != null && user.FailedLoginAttempts > result.MaxLoginAttempt)
                {
                    user.IsLocked = true;
                    Update(user);
                }
                _userLoginReportRepository.SaveLog("Invalid Password and Login Attempt : " + user.FailedLoginAttempts,
                    user.Id, userName);
                return null;
            }
            return user;
        }

        private User validateClient(string userName, string password)
        {

            var user = All.Where(x =>
                (x.UserName == userName || x.Email == userName)
                && x.DeletedDate == null).FirstOrDefault();

            if (user == null)
            {
                _userLoginReportRepository.SaveLog("Invalid User Name", null, userName);
                return null;
            }
            if (!string.IsNullOrEmpty(_userPasswordRepository.VaidatePassword(password, user.Id)))
            {
                user.FailedLoginAttempts++;
                var result = _loginPreferenceRepository.FindBy(x => x.CompanyId == user.CompanyId).FirstOrDefault();
                if (result != null && user.FailedLoginAttempts > result.MaxLoginAttempt)
                {
                    user.IsLocked = true;
                    Update(user);
                }
                _userLoginReportRepository.SaveLog("Invalid Password and Login Attempt : " + user.FailedLoginAttempts,
                    user.Id, userName);
                return null;
            }
            return user;
        }

        public string DuplicateUserName(User objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.FirstName == objSave.FirstName && x.MiddleName == objSave.MiddleName && x.LastName == objSave.LastName && x.Email == objSave.Email && x.DeletedDate == null))
                return "Duplicate User";

            if (All.Any(x => x.Id != objSave.Id && x.UserName == objSave.UserName && x.DeletedDate == null))
                return "Duplicate User Name : " + objSave.UserName;

            //if (All.Any(x => x.Id != objSave.Id && x.Email == objSave.Email && x.DeletedDate == null))
            //    return "Duplicate EMail : " + objSave.Email;

            return "";
        }

        public void UpdateUserStatus(int id)
        {
            var user = Find(id);
            if (user == null) return;
            if (user.DeletedDate != null)
            {
                user.DeletedBy = null;
                user.DeletedDate = null;
                Update(user);
            }
            else
            {
                Delete(user);
            }
        }

        public List<DropDownDto> GetUserName()
        {
            var result = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.UserName }).OrderBy(o => o.Value) //c.FirstName + " " + c.LastName // changed by Neel for trainer dropdown
                .ToList();
            return result;
        }

        public List<DropDownDto> GetUserNameDropdown()
        {
            var result = All.Where(x =>
                   (x.DeletedDate != null) || x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.FirstName + " " + c.LastName }).OrderBy(o => o.Value)
                .ToList();
            return result;
        }

        public RefreshTokenDto Refresh(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);

            var login = _context.RefreshToken.FirstOrDefault(t =>
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

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public void UpdateRefreshToken(int userid, string refreshToken)
        {
            var rereshTokens = _refreshTokenRepository.FindBy(t => t.UserId == userid).ToList();

            rereshTokens.ForEach(x =>
            {
                if (x.ExpiredOn < DateTime.UtcNow)
                    _refreshTokenRepository.Remove(x);
            });

            var rereshToken = new RefreshToken();
            rereshToken.UserId = userid;
            rereshToken.Token = refreshToken;
            rereshToken.ExpiredOn = DateTime.UtcNow.AddDays(1);
            _refreshTokenRepository.Add(rereshToken);
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
    }
}
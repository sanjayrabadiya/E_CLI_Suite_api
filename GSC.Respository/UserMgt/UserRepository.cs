using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.LogReport;
using GSC.Shared;
using GSC.Shared.Configuration;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Serilog;

namespace GSC.Respository.UserMgt
{
    public class UserRepository : GenericRespository<User>, IUserRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        private readonly IGSCContext _context;
        private readonly ICompanyRepository _companyRepository;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly ICentreUserService _centreUserService;
        private readonly IMapper _mapper;
        public UserRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IUserLoginReportRespository userLoginReportRepository,
            ICompanyRepository companyRepository,
             IAppSettingRepository appSettingRepository,
             IUploadSettingRepository uploadSettingRepository,
             IRolePermissionRepository rolePermissionRepository,
             IUserRoleRepository userRoleRepository, IOptions<EnvironmentSetting> environmentSetting, ICentreUserService centreUserService,
             IMapper mapper)
            : base(context)
        {
            _userLoginReportRepository = userLoginReportRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _companyRepository = companyRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _appSettingRepository = appSettingRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _userRoleRepository = userRoleRepository;
            _environmentSetting = environmentSetting;
            _centreUserService = centreUserService;
            _mapper = mapper;
        }

        public async Task<List<UserGridDto>> GetUsers(bool isDeleted)
        {

            var result = await _centreUserService.GetLockedUsers($"{_environmentSetting.Value.CentralApi}User/GetLockedUsers/{_jwtTokenAccesser.CompanyId}");

            var lockUserIds = result.Data.Select(s => s.UserName);

            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null && !lockUserIds.Contains(x.UserName)) && x.UserType != UserMasterUserType.Patient).
                   ProjectTo<UserGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<UserGridDto> GetPatients(PatientDto userDto)
        {

            var UserID = _context.Randomization.Where(x => x.ProjectId == userDto.ProjectId).Select(x => x.UserId).ToList();
            return All.Where(x => (userDto.IsDeleted ? x.DeletedDate != null : x.DeletedDate == null) && (UserID.Contains(x.Id))).
                   ProjectTo<UserGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }



        public string DuplicateUserName(User objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.FirstName == objSave.FirstName.Trim() && x.MiddleName == objSave.MiddleName.Trim() && x.LastName == objSave.LastName.Trim() && x.Email == objSave.Email && x.DeletedDate == null))
                return "Duplicate User";

            if (All.Any(x => x.Id != objSave.Id && x.UserName == objSave.UserName.Trim() && x.DeletedDate == null))
                return "Duplicate User Name : " + objSave.UserName;

            return "";
        }

        public List<DropDownDto> GetUserName()
        {
            var result = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null 
                    && x.UserType != UserMasterUserType.Patient && x.UserType != UserMasterUserType.LAR)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.UserName }).OrderBy(o => o.Value)
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
        public LoginResponseDto GetLoginDetails()
        {
            var roleTokenId = Guid.NewGuid().ToString();
            var user = All.Where(x => x.Id == _jwtTokenAccesser.UserId).FirstOrDefault();

            var login = new LoginResponseDto
            {
                UserName = user.UserName,
                UserId = user.Id,
                RoleTokenId = roleTokenId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                RoleId = _jwtTokenAccesser.RoleId,
                Language = user.Language,
                LanguageShortName = user.Language.ToString(),
                UserType = user.UserType,
                IsFirstTime = user.IsFirstTime
            };

            var imageUrl = _uploadSettingRepository
                .FindBy(x => x.CompanyId == user.CompanyId && x.DeletedDate == null).FirstOrDefault()?.ImageUrl;

            var company = _companyRepository.Find((int)user.CompanyId);
            if (company != null)
            {
                login.CompanyName = company.CompanyName;
                login.CompanyLogo = imageUrl + company.Logo;
                login.UserPicUrl = DocumentService.ConvertBase64Image(imageUrl + (user.ProfilePic ?? DocumentService.DefulatProfilePic));
            }

            login.GeneralSettings = _appSettingRepository.Get<GeneralSettingsDto>(user.CompanyId);
            login.Rights = _rolePermissionRepository.GetByUserId(user.Id, _jwtTokenAccesser.RoleId);
            login.PatientRights = _rolePermissionRepository.GetPatientUserRights(user.Id);
            login.Roles = _userRoleRepository.GetRoleByUserName(user.UserName);
            login.RoleName = login.Roles.FirstOrDefault(t => t.Id == _jwtTokenAccesser.RoleId)?.Value;
            login.LoginReportId =
                     _userLoginReportRepository.SaveLog("Successfully Login", user.Id, user.UserName, _jwtTokenAccesser.RoleId);

            if (user.IsFirstTime)
            {
                user.IsFirstTime = false;
                _context.Users.Update(user);
                _context.Save();
            }
            return login;
        }
        public async Task<UserLockedGridDto> GetLockedUsers()
        {
            var result = await _centreUserService.GetLockedUsers($"{_environmentSetting.Value.CentralApi}User/GetLockedUsers/{_jwtTokenAccesser.CompanyId}");
            return result;
        }

        public void UpdateFirebaseToken(int userId, UserFirebaseToken token)
        {
            var user = Find(userId);
            user.FirebaseToken = token.Token;
            user.DeviceType = token.DeviceType;

            Update(user);
            _context.Save();
        }

        public List<UserUUIDDto> GetUUIDPatients(PatientDto userDto)
        {
            var Users = _context.Randomization.Where(x => x.ProjectId == userDto.ProjectId).Select(x => x.UserId).ToList();
            var userList = All.Where(x => (userDto.IsDeleted ? x.DeletedDate != null : x.DeletedDate == null) && (Users.Contains(x.Id))).
                   ProjectTo<UserGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            List<UserUUIDDto> UUIDuserList = new List<UserUUIDDto>();
            foreach (var item in userList)
            {

                var UUIDUser = _context.UserUUID.Where(x => x.UserId == item.Id && x.UUID != "").ToList();
                foreach (var item1 in UUIDUser)
                {
                    UserUUIDDto obj = new UserUUIDDto();
                    obj.Id = item1.UserId;
                    obj.UUID = item1.UUID;
                    obj.Reason = item1.ReasonId == null ? "" : _context.AuditReason.Find(item1.ReasonId).ReasonName;
                    obj.ReasonOth = item1.ReasonOth;
                    obj.IsActive = item1.Active;
                    obj.UserName = item.UserName;
                    obj.ModifiedByUser = item1.ModifiedBy == null ? "" : _context.Users.Find(item1.ModifiedBy).UserName;
                    obj.ModifiedDate = item1.ModifiedDate;
                    UUIDuserList.Add(obj);
                }
            }

            return UUIDuserList;
        }

        public void ResetUUID(int userId)
        {
            var user = _context.UserUUID.Where(x => x.UserId == userId && x.Active).ToList();

            foreach (var item in user)
            {
                item.Active = false;
                item.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                item.ReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                _context.UserUUID.Update(item);
            }
            _context.Save();
        }
    }
}
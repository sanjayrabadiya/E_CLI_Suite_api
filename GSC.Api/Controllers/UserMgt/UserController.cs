using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Common;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GSC.Shared;
using GSC.Shared.Security;
using GSC.Shared.Generic;
using GSC.Data.Entities;
using GSC.Shared.Configuration;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using GSC.Shared.JWTAuth;
using GSC.Shared.Extension;
using GSC.Respository.LogReport;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using GSC.Respository.Etmf;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly ICentreUserService _centreUserService;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;

        private readonly IProjectSubSecArtificateDocumentApproverRepository _projectSubSecArtificateDocumentApproverRepository;
        private readonly IProjectSubSecArtificateDocumentReviewRepository _projectSubSecArtificateDocumentReviewRepository;
        public UserController(IUserRepository userRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IUploadSettingRepository uploadSettingRepository,
            IUserRoleRepository userRoleRepository,
            IOptions<EnvironmentSetting> environmentSetting,
            ICentreUserService centreUserService,
            IGSCContext context, IJwtTokenAccesser jwtTokenAccesser,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository,
            IProjectSubSecArtificateDocumentApproverRepository projectSubSecArtificateDocumentApproverRepository
            )
        {
            _userRepository = userRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _userRoleRepository = userRoleRepository;
            _environmentSetting = environmentSetting;
            _centreUserService = centreUserService;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _projectSubSecArtificateDocumentApproverRepository = projectSubSecArtificateDocumentApproverRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
        }


        [HttpGet("{isDeleted:bool?}")]
        public async Task<IActionResult> Get(bool isDeleted)
        {
            var usersDto = await _userRepository.GetUsers(isDeleted);
            var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            usersDto.ForEach(t => t.ProfilePicPath = imageUrl + (t.ProfilePic ?? DocumentService.DefulatProfilePic));

            return Ok(usersDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var user = _userRepository.FindByInclude(x => x.Id == id, x => x.UserRoles)
                .FirstOrDefault();

            if (user != null && user.UserRoles != null)
                user.UserRoles = user.UserRoles.Where(x => x.DeletedDate == null).ToList();

            var userDto = _mapper.Map<UserDto>(user);
            var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            userDto.ProfilePicPath = imageUrl + (userDto.ProfilePic ?? DocumentService.DefulatProfilePic);

            return Ok(userDto);
        }

        [HttpPost("Getpatients")]
        public IActionResult Getpatients([FromBody] PatientDto userDto)
        {
            var usersDto = _userRepository.GetPatients(userDto);
            var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            usersDto.ForEach(t => t.ProfilePicPath = imageUrl + (t.ProfilePic ?? DocumentService.DefulatProfilePic));

            return Ok(usersDto);
        }

        [Route("GetLockedUsers")]
        [HttpGet]
        public IActionResult GetLockedUsers()
        {
            var data = _userRepository.GetLockedUsers();
            var imageUrl = _uploadSettingRepository.GetWebImageUrl();
            if (data != null && data.Result != null && data.Result.Data != null)
            {
                foreach (var item in data.Result.Data)
                {
                    var user = _context.Users.Include(x => x.UserRoles).ThenInclude(x => x.SecurityRole).Where(x => x.Id == item.Id).FirstOrDefault();
                    if (user != null && user.UserRoles != null)
                    {
                        item.Role = string.Join(", ", user.UserRoles.Where(x => x.DeletedDate == null).Select(s => s.SecurityRole.RoleName).ToList());
                        item.ProfilePicPath = imageUrl + (user.ProfilePic ?? DocumentService.DefulatProfilePic);
                        item.CreatedByUser = _context.Users.FirstOrDefault(q => q.Id == (user.CreatedBy != null ? user.CreatedBy.Value : 0))?.UserName;
                        item.ModifiedByUser = _context.Users.FirstOrDefault(q => q.Id == (user.ModifiedBy != null ? user.ModifiedBy.Value : 0))?.UserName;
                        item.DeletedByUser = _context.Users.FirstOrDefault(q => q.Id == (user.DeletedBy != null ? user.DeletedBy.Value : 0))?.UserName;
                        item.CreatedDate = user.CreatedDate;
                        item.ModifiedDate = user.ModifiedDate;
                        item.DeletedDate = user.DeletedDate;
                    }
                }
                return Ok(data.Result.Data);
            }
            return Ok(null);
        }



        [HttpGet("GetUserNameByRoleId/{roleId}")]
        public IActionResult GetUserNameByRoleId(int roleId)
        {
            return Ok(_userRoleRepository.GetUserNameByRoleId(roleId));
        }


        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (userDto.FileModel?.Base64?.Length > 0)
                userDto.ProfilePic = new ImageService().ImageSave(userDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.User, FolderType.Logo.GetDescription());

            var user = _mapper.Map<Data.Entities.UserMgt.User>(userDto);
            user.IsFirstTime = true;
            user.UserType = UserMasterUserType.User;

            userDto.UserType = UserMasterUserType.User;
            CommonResponceView userdetails = await _centreUserService.SaveUser(userDto, _environmentSetting.Value.CentralApi);
            if (!string.IsNullOrEmpty(userdetails.Message))
            {
                ModelState.AddModelError("Message", userdetails.Message);
                return BadRequest(ModelState);
            }
            user.Id = Convert.ToInt32(userdetails.Id);

            _userRepository.Add(user);
            user.UserRoles.ForEach(x =>
            {
                _userRoleRepository.Add(x);
            });
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating a User  failed on save.");
                return BadRequest(ModelState);
            }

            return Ok(user.Id);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UserDto userDto)
        {
            if (userDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (userDto.FileModel?.Base64?.Length > 0)
            {
                var userDetails = _userRepository.Find(userDto.Id);
                new ImageService().RemoveImage(_uploadSettingRepository.GetImagePath(), userDetails.ProfilePic);
                userDto.ProfilePic = new ImageService().ImageSave(userDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.User, FolderType.Logo.GetDescription());
            }
            var user = _mapper.Map<Data.Entities.UserMgt.User>(userDto);
            user.IsFirstTime = _userRepository.FindBy(i => i.Id == userDto.Id).FirstOrDefault()?.IsFirstTime ?? false;

            user.UserType = UserMasterUserType.User;
            userDto.UserType = UserMasterUserType.User;
            CommonResponceView userdetails = await _centreUserService.UpdateUser(userDto, _environmentSetting.Value.CentralApi);
            if (!string.IsNullOrEmpty(userdetails.Message))
            {
                ModelState.AddModelError("Message", userdetails.Message);
                return BadRequest(ModelState);
            }

            UpdateRole(user);
            _userRepository.Update(user);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating user failed on save.");
                return BadRequest(ModelState);
            }

            return Ok(user.Id);
        }


        private void UpdateRole(Data.Entities.UserMgt.User user)
        {
            var userrole = _context.UserRole.Where(x => x.UserId == user.Id
                                                               && user.UserRoles.Select(x => x.UserRoleId).Contains(x.UserRoleId)
                                                               && x.DeletedDate == null).ToList();

            user.UserRoles.ForEach(z =>
            {
                var role = userrole.Find(x => x.UserId == user.Id && x.UserRoleId == z.UserRoleId);
                if (role == null)
                {
                    _userRoleRepository.Add(z);
                }
            });

            var userRoles = _context.UserRole.Where(x => x.UserId == user.Id && x.DeletedDate == null)
                .ToList();

            userRoles.ForEach(t =>
            {
                var role = userrole.Find(x => x.UserId == t.UserId && x.UserRoleId == t.UserRoleId);
                if (role == null)
                {
                    //delete
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _userRoleRepository.Update(t);
                }
            });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _centreUserService.DeleteUser(_environmentSetting.Value.CentralApi, id);
            var user = _userRepository.Find(id);
            _userRepository.Delete(user);

            var etmfUserList = _projectWorkplaceArtificateDocumentReviewRepository.All.Where(x => x.UserId == id && x.DeletedDate == null);
            foreach (var etmfRecored in etmfUserList)
            {
                _projectWorkplaceArtificateDocumentReviewRepository.Delete(etmfRecored);
            }

            var etmfapproverUsers = _projectArtificateDocumentApproverRepository.All.Where(x => x.UserId == id && x.DeletedDate == null);
            foreach (var etmfRecored in etmfapproverUsers)
            {
                _projectArtificateDocumentApproverRepository.Delete(etmfRecored);
            }

            var etmfsubreviewerUsers = _projectSubSecArtificateDocumentReviewRepository.All.Where(x => x.UserId == id && x.DeletedDate == null);
            foreach (var etmfRecored in etmfsubreviewerUsers)
            {
                _projectSubSecArtificateDocumentReviewRepository.Delete(etmfRecored);
            }

            var etmfsubapproverUsers = _projectSubSecArtificateDocumentApproverRepository.All.Where(x => x.UserId == id && x.DeletedDate == null);
            foreach (var etmfRecored in etmfsubapproverUsers)
            {
                _projectSubSecArtificateDocumentApproverRepository.Delete(etmfRecored);
            }

            _uow.Save();


            return Ok();
        }



        [HttpPatch("{id}")]
        public async Task<IActionResult> Active(int id)
        {
            var record = _userRepository.Find(id);

            if (record == null)
                return NotFound();

            CommonResponceView userdetails = await _centreUserService.ActiveUser(_environmentSetting.Value.CentralApi, id);
            if (!string.IsNullOrEmpty(userdetails.Message))
            {
                ModelState.AddModelError("Message", userdetails.Message);
                return BadRequest(ModelState);
            }

            _userRepository.Active(record);

            var etmfUserList = _projectWorkplaceArtificateDocumentReviewRepository.All.Where(x => x.UserId == id && x.DeletedDate != null);
            foreach (var etmfRecored in etmfUserList)
            {
                _projectWorkplaceArtificateDocumentReviewRepository.Active(etmfRecored);
            }

            var etmfapproverUsers = _projectArtificateDocumentApproverRepository.All.Where(x => x.UserId == id && x.DeletedDate != null);
            foreach (var etmfRecored in etmfapproverUsers)
            {
                _projectArtificateDocumentApproverRepository.Active(etmfRecored);
            }

            var etmfsubreviewerUsers = _projectSubSecArtificateDocumentReviewRepository.All.Where(x => x.UserId == id && x.DeletedDate != null);
            foreach (var etmfRecored in etmfsubreviewerUsers)
            {
                _projectSubSecArtificateDocumentReviewRepository.Active(etmfRecored);
            }

            var etmfsubapproverUsers = _projectSubSecArtificateDocumentApproverRepository.All.Where(x => x.UserId == id && x.DeletedDate != null);
            foreach (var etmfRecored in etmfsubapproverUsers)
            {
                _projectSubSecArtificateDocumentApproverRepository.Active(etmfRecored);
            }

            _uow.Save();
            return Ok();
        }

        [HttpGet("GetUserName")]
        public IActionResult GetUserName()
        {
            return Ok(_userRepository.GetUserName());
        }

        [HttpGet("GetUserNameDropdown")]
        public IActionResult GetUserNameDropdown()
        {
            return Ok(_userRepository.GetUserNameDropdown());
        }

        [HttpPost]
        [Route("UpdateFirebaseToken")]
        public IActionResult UpdateFirebaseToken([FromBody] UserFirebaseToken token)
        {        
            _userRepository.UpdateFirebaseToken(_jwtTokenAccesser.UserId, token);
            return Ok(1);
        }

        [HttpPost("GetUUIDPatients")]
        public IActionResult GetUUIDPatients([FromBody] PatientDto userDto)
        {
            return Ok(_userRepository.GetUUIDPatients(userDto));
        }

        [HttpPatch("ResetUUID/{id}")]
        public IActionResult ResetUUID(int id)
        {
            var record = _userRepository.Find(id);

            if (record == null)
                return NotFound();
            _userRepository.ResetUUID(id);
            return Ok();
        }
    }
}
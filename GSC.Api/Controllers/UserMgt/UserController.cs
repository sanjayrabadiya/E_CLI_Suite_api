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

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class UserController : BaseController
    {
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly ILocationRepository _locationRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUserPasswordRepository _userPasswordRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;    
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly ICentreUserService _centreUserService;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public UserController(IUserRepository userRepository,
            IUnitOfWork uow,
            IMapper mapper,
            ILocationRepository locationRepository, IUserPasswordRepository userPasswordRepository,
            IEmailSenderRespository emailSenderRespository,
            IUploadSettingRepository uploadSettingRepository,
            IUserRoleRepository userRoleRepository,
            IProjectRepository projectRepository,          
            IOptions<EnvironmentSetting> environmentSetting,
            ICentreUserService centreUserService,
            IGSCContext context, IJwtTokenAccesser jwtTokenAccesser
            )
        {
            _userRepository = userRepository;
            _uow = uow;
            _mapper = mapper;
            _locationRepository = locationRepository;
            _userPasswordRepository = userPasswordRepository;
            _emailSenderRespository = emailSenderRespository;
            _uploadSettingRepository = uploadSettingRepository;
            _userRoleRepository = userRoleRepository;
            _projectRepository = projectRepository;          
            _environmentSetting = environmentSetting;
            _centreUserService = centreUserService;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var usersDto = _userRepository.GetUsers(isDeleted);
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
            //userDto.ProfilePicPath = DocumentService.ConvertBase64Image(imageUrl + (userDto.ProfilePic ?? DocumentService.DefulatProfilePic));
            return Ok(userDto);
        }

        [HttpGet("GetBlockedUser/{id}")]
        public IActionResult GetBlockedUser(int id)
        {
            if (id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var user = _userRepository.FindBy(i => i.Id == id).FirstOrDefault();
            if (user != null && user.IsLocked) user.IsLocked = false;

            _userRepository.Update(user);

            if (_uow.Save() <= 0) throw new Exception("User not unblocked.");
            return Ok(id);
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
                    _uploadSettingRepository.GetImagePath(), FolderType.Employee);

            var user = _mapper.Map<Data.Entities.UserMgt.User>(userDto);
            user.IsLocked = false;
            user.IsFirstTime = true;
            user.UserType = UserMasterUserType.User;
            bool IsPremise = _environmentSetting.Value.IsPremise;
            if (!IsPremise)
            {
                userDto.UserType = UserMasterUserType.User;
                CommonResponceView userdetails = await _centreUserService.SaveUser(userDto, _environmentSetting.Value.CentralApi);
                if (!string.IsNullOrEmpty(userdetails.Message))
                {
                    ModelState.AddModelError("Message", userdetails.Message);
                    return BadRequest(ModelState);
                }
                user.Id = Convert.ToInt32(userdetails.Id);
            }
            else
            {
                var validate = _userRepository.DuplicateUserName(user);
                if (!string.IsNullOrEmpty(validate))
                {
                    ModelState.AddModelError("Message", validate);
                    return BadRequest(ModelState);
                }
            }
            _userRepository.Add(user);
            user.UserRoles.ForEach(x =>
            {
                _userRoleRepository.Add(x);
            });
            if (_uow.Save() <= 0) throw new Exception("Creating a User  failed on save.");

            if (IsPremise)
            {
                var password = RandomPassword.CreateRandomPassword(6);
                _userPasswordRepository.CreatePassword(password, user.Id);
                _emailSenderRespository.SendRegisterEMail(user.Email, password, user.UserName);
            }
            return Ok(user.Id);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UserDto userDto)
        {
            if (userDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (userDto.FileModel?.Base64?.Length > 0)
                userDto.ProfilePic = new ImageService().ImageSave(userDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.Employee);

            var user = _mapper.Map<Data.Entities.UserMgt.User>(userDto);
            user.IsFirstTime = _userRepository.FindBy(i => i.Id == userDto.Id).FirstOrDefault()?.IsFirstTime ?? false;
            user.IsLocked = _userRepository.FindBy(i => i.Id == userDto.Id).FirstOrDefault()?.IsLocked ?? false;
            user.UserType = UserMasterUserType.User;
            bool IsPremise = _environmentSetting.Value.IsPremise;
            if (!IsPremise)
            {
                userDto.UserType = UserMasterUserType.User;
                CommonResponceView userdetails = await _centreUserService.UpdateUser(userDto, _environmentSetting.Value.CentralApi);
                if (!string.IsNullOrEmpty(userdetails.Message))
                {
                    ModelState.AddModelError("Message", userdetails.Message);
                    return BadRequest(ModelState);
                }
            }
            else
            {
                var validate = _userRepository.DuplicateUserName(user);
                if (!string.IsNullOrEmpty(validate))
                {
                    ModelState.AddModelError("Message", validate);
                    return BadRequest(ModelState);
                }
            }
            UpdateRole(user);
            _userRepository.Update(user);
            if (_uow.Save() <= 0) throw new Exception("Updating user failed on save.");

            return Ok(user.Id);
        }


        private void UpdateRole(Data.Entities.UserMgt.User user)
        {
            var userrole = _context.UserRole.Where(x => x.UserId == user.Id
                                                               && user.UserRoles.Select(x => x.UserRoleId).Contains(x.UserRoleId)
                                                               && x.DeletedDate == null).ToList();

            user.UserRoles.ForEach(z =>
            {
                var role = userrole.Where(x => x.UserId == user.Id && x.UserRoleId == z.UserRoleId).FirstOrDefault();
                if (role == null)
                {
                    _userRoleRepository.Add(z);
                }
            });

            var userRoles = _context.UserRole.Where(x => x.UserId == user.Id && x.DeletedDate == null)
                .ToList();

            userRoles.ForEach(t =>
            {
                var role = userrole.Where(x => x.UserId == t.UserId && x.UserRoleId == t.UserRoleId).FirstOrDefault();
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
            if (!_environmentSetting.Value.IsPremise)
            {
                _centreUserService.DeleteUser(_environmentSetting.Value.CentralApi, id);
            }
            var user = _userRepository.Find(id);
            _userRepository.Delete(user);
            _uow.Save();

            return Ok();
        }

        [HttpPost]
        [Route("ChangePassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto loginDto)
        {

            var user = _userRepository.FindBy(x => x.UserName == loginDto.UserName && x.DeletedDate == null)
                .FirstOrDefault();
            if (user == null)
                return NotFound();
            if (!_environmentSetting.Value.IsPremise)
            {
                CommonResponceView userdetails = await _centreUserService.ChangePassword(loginDto, _environmentSetting.Value.CentralApi);
                if (!string.IsNullOrEmpty(userdetails.Message))
                {
                    ModelState.AddModelError("Message", userdetails.Message);
                    return BadRequest(ModelState);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_userPasswordRepository.VaidatePassword(loginDto.OldPassword, user.Id)))
                {
                    ModelState.AddModelError("Message", "Current Password invalid!");
                    return BadRequest(ModelState);
                }
            }
            user.IsFirstTime = false;
            user.IsLogin = false;
            _userRepository.Update(user);
            _uow.Save();
            _userPasswordRepository.CreatePassword(loginDto.NewPassword, user.Id);
            _emailSenderRespository.SendRegisterEMail(user.Email, loginDto.NewPassword, user.UserName);
            return Ok();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Active(int id)
        {
            var record = _userRepository.Find(id);

            if (record == null)
                return NotFound();
            if (!_environmentSetting.Value.IsPremise)
            {
                CommonResponceView userdetails = await _centreUserService.ActiveUser(_environmentSetting.Value.CentralApi, id);
                if (!string.IsNullOrEmpty(userdetails.Message))
                {
                    ModelState.AddModelError("Message", userdetails.Message);
                    return BadRequest(ModelState);
                }
            }
            else
            {
                var validate = _userRepository.DuplicateUserName(record);
                if (!string.IsNullOrEmpty(validate))
                {
                    ModelState.AddModelError("Message", validate);
                    return BadRequest(ModelState);
                }
            }
            _userRepository.Active(record);
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
    }
}
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
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly IAPICall _centerEndpoint;

        public UserController(IUserRepository userRepository,
            IUnitOfWork uow,
            IMapper mapper,
            ILocationRepository locationRepository, IUserPasswordRepository userPasswordRepository,
            IEmailSenderRespository emailSenderRespository,
            IUploadSettingRepository uploadSettingRepository,
            IUserRoleRepository userRoleRepository,
            IProjectRepository projectRepository, 
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            IAPICall centerEndpoint
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
            _configuration = configuration;
            _centerEndpoint = centerEndpoint;
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
            //userDto.ProfilePicPath = imageUrl + (userDto.ProfilePic ?? DocumentService.DefulatProfilePic);
            userDto.ProfilePicPath = DocumentService.ConvertBase64Image(imageUrl + (userDto.ProfilePic ?? DocumentService.DefulatProfilePic));
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
        public IActionResult Post([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (userDto.FileModel?.Base64?.Length > 0)
                userDto.ProfilePic = new ImageService().ImageSave(userDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.Employee);

            var user = _mapper.Map<User>(userDto);
            user.IsLocked = false;
            user.IsFirstTime = true;
            bool IsCloud = Convert.ToBoolean(_configuration["IsCloud"]);
            if (IsCloud)
            {
                userDto.UserType = UserMasterUserType.User;
                var response = _centerEndpoint.Post(userDto, $"{_configuration["EndPointURL"]}/User");
                //if (!response.IsSuccessStatusCode)
                //    return BadRequest(response.Content.ReadAsStringAsync().Result);
                //var Id = response.Content.ReadAsStringAsync().Result;
                user.Id = Convert.ToInt32(response);
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
            if (_uow.Save() <= 0) throw new Exception("Creating a User  failed on save.");

            if (!IsCloud)
            {
                var password = RandomPassword.CreateRandomPassword(6);
                _userPasswordRepository.CreatePassword(password, user.Id);
                _emailSenderRespository.SendRegisterEMail(user.Email, password, user.UserName);
            }
            return Ok(user.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] UserDto userDto)
        {
            if (userDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (userDto.FileModel?.Base64?.Length > 0)
                userDto.ProfilePic = new ImageService().ImageSave(userDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.Employee);

            var user = _mapper.Map<User>(userDto);
            user.IsFirstTime = _userRepository.FindBy(i => i.Id == userDto.Id).FirstOrDefault()?.IsFirstTime ?? false;
            user.IsLocked = _userRepository.FindBy(i => i.Id == userDto.Id).FirstOrDefault()?.IsLocked ?? false;

            bool IsCloud = Convert.ToBoolean(_configuration["IsCloud"]);
            if (IsCloud)
            {
                userDto.UserType = UserMasterUserType.User;
                var response = _centerEndpoint.Put(userDto, $"{_configuration["EndPointURL"]}/User");
                //if (!response.IsSuccessStatusCode)
                //    return BadRequest(response.Content.ReadAsStringAsync().Result);
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


        private void UpdateRole(User user)
        {
            var roleDelete = _userRoleRepository.FindBy(x => x.UserId == user.Id)
                .ToList();
            foreach (var item in roleDelete)
            {
                item.DeletedDate = DateTime.Now;
                _userRoleRepository.Update(item);
            }

            for (var i = 0; i < user.UserRoles.Count; i++)
            {
                var i1 = i;
                var userrole = _userRoleRepository.FindBy(x => x.UserRoleId == user.UserRoles[i1].UserRoleId
                                                               && x.UserId == user.UserRoles[i1].UserId)
                    .FirstOrDefault();
                if (userrole != null)
                {
                    userrole.DeletedDate = null;
                    userrole.DeletedBy = null;
                    user.UserRoles[i] = userrole;
                }
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (Convert.ToBoolean(_configuration["IsCloud"]))
            {
                var response = _centerEndpoint.Delete($"{_configuration["EndPointURL"]}/User/{id}");
                //if (!response.IsSuccessStatusCode)
                //    return BadRequest(response.Content.ReadAsStringAsync().Result);              
            }
            var user = _userRepository.Find(id);
            _userRepository.Delete(user);
            _uow.Save();

            return Ok();
        }

        [HttpPost]
        [Route("ChangePassword")]
        [AllowAnonymous]
        public IActionResult ChangePassword([FromBody] ChangePasswordDto loginDto)
        {
            //var user = _userRepository.FindBy(x => x.UserName == loginDto.UserName && x.DeletedDate == null)
            //    .FirstOrDefault();
            //if (user == null)
            //    return NotFound();

            //if (!string.IsNullOrEmpty(_userPasswordRepository.VaidatePassword(loginDto.OldPassword, user.Id)))
            //{
            //    ModelState.AddModelError("Message", "Current Password invalid!");
            //    return BadRequest(ModelState);
            //}

            //user.IsFirstTime = false;
            //user.IsLogin = false;
            //_userRepository.Update(user);


            //_uow.Save();
            //harshil

            var user = _userRepository.FindBy(x => x.UserName == loginDto.UserName && x.DeletedDate == null)
                .FirstOrDefault();
            if (user == null)
                return NotFound();
            if (Convert.ToBoolean(_configuration["IsCloud"]))
            {
                var response = _centerEndpoint.Post(loginDto, $"{_configuration["EndPointURL"]}/User");
                //if (!response.IsSuccessStatusCode)
                //    return BadRequest(response.Content.ReadAsStringAsync().Result);
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

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _userRepository.Find(id);

            if (record == null)
                return NotFound();
            if (Convert.ToBoolean(_configuration["IsCloud"]))
            {               
                var responce= _centerEndpoint.Patch($"{_configuration["EndPointURL"]}/User/{id}",null);
                //if (!response.IsSuccessStatusCode)
                //    return BadRequest(response.Content.ReadAsStringAsync().Result);
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
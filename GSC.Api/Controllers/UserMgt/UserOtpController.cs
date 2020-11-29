using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using FluentValidation.Resources;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.UserMgt;
using GSC.Shared.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class UserOtpController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUserOtpRepository _userOtpRepository;
        private readonly IUserRepository _userRepository;

        public UserOtpController(IUserOtpRepository userOtpRepository,
            IUnitOfWork uow,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _userOtpRepository = userOtpRepository;
            _uow = uow;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("InsertOtp/{userName}")]
        public IActionResult InsertOtp(string userName)
        {
            var validateMessage = _userOtpRepository.InsertOtp(userName);

            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }

            _uow.Save();
            return Ok(new { message = "User verified successfullly!", StatusCode = 200 });
        }

        [HttpPost]
        [Route("VerifyOtp")]
        [AllowAnonymous]
        public IActionResult VerifyOtp([FromBody] UserOtpDto userOtpDto)
        {
            var validateMessage = _userOtpRepository.VerifyOtp(userOtpDto);

            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }

            _uow.Save();
            return Ok(new { message = "OTP verified successfullly!", StatusCode = 200 });
        }

        [HttpPost]
        [Route("ChangePasswordByOtp")]
        [AllowAnonymous]
        public IActionResult ChangePasswordByOtp([FromBody] UserOtpDto userOtpDto)
        {
            var validateMessage = _userOtpRepository.ChangePasswordByOtp(userOtpDto);
            if (validateMessage == "")
            {
                var user = _userRepository.FindBy(x => x.UserName == userOtpDto.UserName && x.DeletedDate == null)
                    .FirstOrDefault();
                if (user != null)
                {
                    user.IsFirstTime = false;
                    var userupdate = _mapper.Map<Data.Entities.UserMgt.User>(user);

                    _userRepository.Update(userupdate);
                }

                if (_uow.Save() <= 0) throw new Exception("Updating user failed on change password.");
            }

            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }

            _uow.Save();
            return Ok(new { message = "Password reset successfullly!", StatusCode = 200 });
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("VerifyingUser/{userName}")]
        public IActionResult VerifyingUser(string userName)
        {
            var userExists = _userRepository.All.Where(x => x.UserName == userName || x.Phone == userName).FirstOrDefault();
            if (userExists == null)
            {
                ModelState.AddModelError("Message", "UserName not valid");
                return BadRequest(ModelState);
            }

            var userDto = _mapper.Map<UserDto>(userExists);
            if (userDto.Language == null)
                userDto.LanguageShortName = PrefLanguage.en.ToString();
            else
                userDto.LanguageShortName = userDto.Language.ToString();

            if (userExists.IsFirstTime)
            {
                var validateMessage = _userOtpRepository.InsertOtp(userName);

                if (!string.IsNullOrEmpty(validateMessage))
                {
                    ModelState.AddModelError("Message", validateMessage);
                    return BadRequest(ModelState);
                }
                _uow.Save();
                return Ok(userDto);
            }
            else
                return Ok(userDto);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("VerifyingMobileUser/{userName}")]
        public IActionResult VerifyingMobileUser(string userName)
        {
            var userExists = _userRepository.All.Where(x => x.UserName == userName || x.Phone == userName).FirstOrDefault();
            if (userExists == null)
            {
                ModelState.AddModelError("Message", "UserName not valid");
                return BadRequest(ModelState);
            }

            var userDto = _mapper.Map<UserMobileDto>(userExists);

            if (Convert.ToBoolean(userDto.IsLocked) == true)
            {
                userDto.IsLocked = "User is locked, Please contact your administrator";
            }
            else
            {
                userDto.IsLocked = "false";
            }

            if (userExists.ValidFrom.HasValue && userExists.ValidFrom.Value > DateTime.Now ||
            userExists.ValidTo.HasValue && userExists.ValidTo.Value < DateTime.Now)
            {
                userDto.IsActive = "User not active, Please contact your administrator";
            }
            else
            {
                userDto.IsActive = "false";
            }

            if (userExists.DeletedDate == null)
            {
                userDto.IsDeleted = "false";
            }
            else
            {
                userDto.IsDeleted = "User is deleted, Please contact your administrator";
            }
            if (userDto.Language == null)
                userDto.LanguageShortName = PrefLanguage.en.ToString();
            else
                userDto.LanguageShortName = userDto.Language.ToString();

            if (userExists.IsFirstTime)
            {
                var validateMessage = _userOtpRepository.InsertOtp(userName);

                if (!string.IsNullOrEmpty(validateMessage))
                {
                    ModelState.AddModelError("Message", validateMessage);
                    return BadRequest(ModelState);
                }
                _uow.Save();
                    return Ok(userDto);
            }
            else
                return Ok(userDto);
        }

        [HttpPut]
        [AllowAnonymous]
        [Route("UserDeleteTempForMobile/{userName}")]
        public IActionResult UserDeleteTempForMobile(string userName)
        {
            var userExists = _userRepository.All.Where(x => x.UserName == userName || x.Phone == userName).FirstOrDefault();
            if (userExists == null)
            {
                ModelState.AddModelError("Message", "UserName not valid");
                return BadRequest(ModelState);
            }

            var user = _mapper.Map<Data.Entities.UserMgt.User>(userExists);
            user.IsFirstTime = true;

            _userRepository.Update(user);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "User not deleted");
                return BadRequest(ModelState);
            }
            return Ok(new { message = "User deleted successfullly!", StatusCode = 200 });
        }
    }
}
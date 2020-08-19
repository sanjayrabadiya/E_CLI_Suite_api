using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.UserMgt;
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
            return Ok(new { message = "User varified successfullly!", StatusCode = 200 });
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
            return Ok(new{message="OTP varified successfullly!",StatusCode = 200});
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
                    var userupdate = _mapper.Map<User>(user);

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
    }
}
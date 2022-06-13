﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation.Resources;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.UserMgt;
using GSC.Shared.Configuration;
using GSC.Shared.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly ICentreUserService _centreUserService;
        public UserOtpController(IUserOtpRepository userOtpRepository,
            IUnitOfWork uow,
            IUserRepository userRepository,
            IMapper mapper, IOptions<EnvironmentSetting> environmentSetting, ICentreUserService centreUserService)
        {
            _userOtpRepository = userOtpRepository;
            _uow = uow;
            _mapper = mapper;
            _userRepository = userRepository;
            _environmentSetting = environmentSetting;
            _centreUserService = centreUserService;
        }

        [HttpPut]
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
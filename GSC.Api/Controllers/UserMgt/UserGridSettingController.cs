using System;
using System.Collections.Generic;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class UserGridSettingController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUserGridSettingRepository _userGridSettingRepository;

        public UserGridSettingController(IUserGridSettingRepository userGridSettingRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper,
            IUnitOfWork uow)
        {
            _userGridSettingRepository = userGridSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _uow = uow;
        }

        [HttpGet("{screenCode}")]
        public IActionResult Get(string screenCode)
        {
            var gridSettings =
                _userGridSettingRepository.FindBy(t =>
                    t.ScreenCode == screenCode && t.UserId == _jwtTokenAccesser.UserId);
            var gridSettingsDto = _mapper.Map<IEnumerable<UserGridSettingDto>>(gridSettings);

            return Ok(gridSettingsDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<UserGridSettingDto> gridSettingsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            gridSettingsDto.ForEach(t =>
            {
                var userGridSetting = _mapper.Map<UserGridSetting>(t);
                userGridSetting.UserId = _jwtTokenAccesser.UserId;

                if (userGridSetting.Id > 0)
                    _userGridSettingRepository.Update(userGridSetting);
                else
                    _userGridSettingRepository.Add(userGridSetting);
            });

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Saving grid settings failed.");
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}
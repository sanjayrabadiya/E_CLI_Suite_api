using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class UploadSettingController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public UploadSettingController(IUploadSettingRepository uploadSettingRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var uploadSettings = _uploadSettingRepository.All.Where(x =>
                x.CompanyId == _jwtTokenAccesser.CompanyId
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var uploadSettingsDto = _mapper.Map<IEnumerable<UploadSettingDto>>(uploadSettings);
            return Ok(uploadSettingsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var uploadSetting = _uploadSettingRepository.Find(id);
            var uploadSettingDto = _mapper.Map<UploadSettingDto>(uploadSetting);
            return Ok(uploadSettingDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] UploadSettingDto uploadSettingDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            uploadSettingDto.Id = 0;
            var uploadSetting = _mapper.Map<UploadSetting>(uploadSettingDto);
            _uploadSettingRepository.Add(uploadSetting);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Upload Setting failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(uploadSetting.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] UploadSettingDto uploadSettingDto)
        {
            if (uploadSettingDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var uploadSetting = _mapper.Map<UploadSetting>(uploadSettingDto);

            _uploadSettingRepository.Update(uploadSetting);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Upload Setting failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(uploadSetting.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _uploadSettingRepository.Find(id);

            if (record == null)
                return NotFound();

            _uploadSettingRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _uploadSettingRepository.Find(id);

            if (record == null)
                return NotFound();
            _uploadSettingRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("IsUnlimitedUploadlimit")]
        public IActionResult IsUnlimitedUploadlimit()
        {
            var uploadSetting = _uploadSettingRepository.IsUnlimitedUploadlimit();
            return Ok(uploadSetting);
        }
    }
}
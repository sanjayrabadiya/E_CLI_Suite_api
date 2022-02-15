using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.GeneralConfig;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Respository.Project.GeneralConfig;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Project.GeneralConfig
{
    [Route("api/[controller]")]
    public class CtmsSettingsController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ICtmsSettingsRepository _ctmsSettingsRepository;
        public CtmsSettingsController(
            IUnitOfWork uow, IMapper mapper, ICtmsSettingsRepository ctmsSettingsRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _ctmsSettingsRepository = ctmsSettingsRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var ctmsSettings = _ctmsSettingsRepository.FindBy(x => x.ProjectId == id).FirstOrDefault();
            var ctmsSettingsDto = _mapper.Map<CtmsSettingsDto>(ctmsSettings);
            return Ok(ctmsSettingsDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] CtmsSettingsDto ctmsSettingsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsSettingsDto.Id = 0;
            var ctmsSettings = _mapper.Map<CtmsSettings>(ctmsSettingsDto);

            _ctmsSettingsRepository.Add(ctmsSettings);
            if (_uow.Save() <= 0) throw new Exception("Creating ctms settings failed on save.");
            return Ok(ctmsSettings.Id);
        }
        [HttpPut]
        public IActionResult Put([FromBody] CtmsSettingsDto ctmsSettingsDto)
        {
            if (ctmsSettingsDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ctmsSettings = _mapper.Map<CtmsSettings>(ctmsSettingsDto);

            _ctmsSettingsRepository.Update(ctmsSettings);

            if (_uow.Save() <= 0) throw new Exception("Update ctms settings failed on save.");
            return Ok(ctmsSettings.Id);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Entities.AdverseEvent;
using GSC.Respository.AdverseEvent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.AdverseEvent
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdverseEventSettingsController : ControllerBase
    {
        private readonly IAdverseEventSettingsRepository _adverseEventSettingsRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public AdverseEventSettingsController(IAdverseEventSettingsRepository adverseEventSettingsRepository,
            IMapper mapper,
            IUnitOfWork uow)
        {
            _adverseEventSettingsRepository = adverseEventSettingsRepository;
            _mapper = mapper;
            _uow = uow;
        }

        [HttpGet("{projectId}")]
        public IActionResult Get(int projectId)
        {
            if (projectId <= 0)
            {
                return BadRequest();
            }
            var adverseEventSettings = _adverseEventSettingsRepository.FindBy(x => x.ProjectId == projectId).ToList().FirstOrDefault();
            var adverseEventSettingsDto = _mapper.Map<AdverseEventSettingsDto>(adverseEventSettings);
            return Ok(adverseEventSettingsDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] AdverseEventSettingsDto adverseEventSettingsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var existingdata = _adverseEventSettingsRepository.FindBy(x => x.ProjectId == adverseEventSettingsDto.ProjectId).ToList();
            if (existingdata != null && existingdata.Count > 0)
            {
                throw new Exception("Error to save Adverse Event settings.");
            }
            var adverseEventSettings = _mapper.Map<AdverseEventSettings>(adverseEventSettingsDto);
            _adverseEventSettingsRepository.Add(adverseEventSettings);
            if (_uow.Save() <= 0) throw new Exception("Error to save Adverse Event settings.");
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] AdverseEventSettingsDto adverseEventSettingsDto)
        {
            if (adverseEventSettingsDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var adverseEventSettings = _mapper.Map<AdverseEventSettings>(adverseEventSettingsDto);
            _adverseEventSettingsRepository.Update(adverseEventSettings);
            if (_uow.Save() <= 0) throw new Exception("Error to save Adverse Event settings.");
            return Ok();
        }

    }


}

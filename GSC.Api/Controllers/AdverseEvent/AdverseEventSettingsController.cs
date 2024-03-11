using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.AdverseEvent;
using GSC.Data.Entities.AdverseEvent;
using GSC.Domain.Context;
using GSC.Respository.AdverseEvent;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.AdverseEvent
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdverseEventSettingsController : ControllerBase
    {
        private readonly IAdverseEventSettingsRepository _adverseEventSettingsRepository;
        private readonly IAEReportingRepository _aEReportingRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IAdverseEventSettingsDetailRepository _adverseEventSettingsDetailRepository;
        public AdverseEventSettingsController(IAdverseEventSettingsRepository adverseEventSettingsRepository,
            IMapper mapper,
            IUnitOfWork uow, IAEReportingRepository aEReportingRepository, IAdverseEventSettingsDetailRepository adverseEventSettingsDetailRepository)
        {
            _adverseEventSettingsRepository = adverseEventSettingsRepository;
            _mapper = mapper;
            _uow = uow;
            _aEReportingRepository = aEReportingRepository;
            _adverseEventSettingsDetailRepository = adverseEventSettingsDetailRepository;

        }

        [HttpGet("{projectId}")]
        public IActionResult Get(int projectId)
        {
            if (projectId <= 0)
            {
                return BadRequest();
            }
            var adverseEventSettingsDto = _adverseEventSettingsRepository.GetData(projectId);
            return Ok(adverseEventSettingsDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] AdverseEventSettingsDto adverseEventSettingsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var existingdata = _adverseEventSettingsRepository.All.Where(x => x.ProjectId == adverseEventSettingsDto.ProjectId && x.DeletedDate == null).ToList();
            if (existingdata != null && existingdata.Count > 0)
            {
                ModelState.AddModelError("Message", "Error to save Adverse Event settings.");
                return BadRequest(ModelState);
            }
            if (adverseEventSettingsDto.adverseEventSettingsDetails == null)
            {
                ModelState.AddModelError("Message", "Severity is mandatory to save!");
                return BadRequest(ModelState);
            }
            var adverseEventSettings = _mapper.Map<AdverseEventSettings>(adverseEventSettingsDto);
            _adverseEventSettingsRepository.Add(adverseEventSettings);

            foreach (var item in adverseEventSettingsDto.adverseEventSettingsDetails)
            {
                item.AdverseEventSettingsId = adverseEventSettings.Id;
                _adverseEventSettingsDetailRepository.Add(item);
            }
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Error to save Adverse Event settings.");
                return BadRequest(ModelState);
            }
            return Ok(adverseEventSettings.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] AdverseEventSettingsDto adverseEventSettingsDto)
        {
            if (adverseEventSettingsDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var existingdata = _aEReportingRepository.All.Where(x => x.AdverseEventSettingsId == adverseEventSettingsDto.Id && x.DeletedDate == null).FirstOrDefault();
            if (existingdata != null)
            {
                ModelState.AddModelError("Message", "Patient already reported.You can not modify the data!");
                return BadRequest(ModelState);
            }
            if (adverseEventSettingsDto.adverseEventSettingsDetails == null)
            {
                ModelState.AddModelError("Message", "Severity is mandatory to save!");
                return BadRequest(ModelState);
            }
            var adverseEventSettings = _mapper.Map<AdverseEventSettings>(adverseEventSettingsDto);
            _adverseEventSettingsRepository.Update(adverseEventSettings);
            _adverseEventSettingsRepository.RemoveExistingAdverseDetail(adverseEventSettings.Id);
            foreach (var item in adverseEventSettingsDto.adverseEventSettingsDetails)
            {
                item.AdverseEventSettingsId = adverseEventSettings.Id;
                _adverseEventSettingsDetailRepository.Add(item);
            }
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Error to save Adverse Event settings.");
                return BadRequest(ModelState);
            }
            return Ok(adverseEventSettings.Id);
        }


        [HttpGet]
        [Route("GetVisitDropDownforAEReportingInvestigatorForm/{projectId}")]
        public IActionResult GetVisitDropDownforAEReportingInvestigatorForm(int projectId)
        {
            return Ok(_adverseEventSettingsRepository.GetVisitDropDownforAEReportingInvestigatorForm(projectId));
        }

        [HttpGet]
        [Route("GetTemplateDropDownforPatientAEReporting/{projectId}")]
        public IActionResult GetTemplateDropDownforPatientAEReporting(int projectId)
        {
            return Ok(_adverseEventSettingsRepository.GetTemplateDropDownforPatientAEReporting(projectId));
        }

        [HttpGet]
        [Route("GetTemplateDropDownforInvestigatorAEReporting/{visitId}")]
        public IActionResult GetTemplateDropDownforInvestigatorAEReporting(int visitId)
        {
            return Ok(_adverseEventSettingsRepository.GetTemplateDropDownforInvestigatorAEReporting(visitId));
        }

        [HttpGet]
        [Route("GetAdverseEventSettingsVariableValue/{projectDesignTemplateId}")]
        public IActionResult GetAdverseEventSettingsVariableValue(int projectDesignTemplateId)
        {
            if (!_adverseEventSettingsRepository.IsvalidPatientTemplate(projectDesignTemplateId))
            {
                ModelState.AddModelError("Message", "Patient Template Is Not Valid!");
                return BadRequest(ModelState);
            }
            var data = _adverseEventSettingsRepository.GetAdverseEventSettingsVariableValue(projectDesignTemplateId);
            if (data == null)
            {
                ModelState.AddModelError("Message", "Please set variable for this template!");
                return BadRequest(ModelState);
            }
            return Ok(data);
        }

    }


}

using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateVariableSequenceNoSettingController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ITemplateVariableSequenceNoSettingRepository _templateVariableSequenceNoSettingRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public TemplateVariableSequenceNoSettingController(ITemplateVariableSequenceNoSettingRepository templateVariableSequenceNoSettingRepository,
            IUnitOfWork uow, IMapper mapper,
            IGSCContext context,
            IStudyVersionRepository studyVersionRepository)
        {
            _templateVariableSequenceNoSettingRepository = templateVariableSequenceNoSettingRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var templateVariableSettings = _templateVariableSequenceNoSettingRepository.FindBy(x => x.ProjectDesignId == id && x.DeletedDate==null).FirstOrDefault();
            var templateVariableSettingsDto = _mapper.Map<TemplateVariableSequenceNoSettingDto>(templateVariableSettings);
            return Ok(templateVariableSettingsDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TemplateVariableSequenceNoSettingDto templateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            templateDto.Id = 0;
            var template = _mapper.Map<TemplateVariableSequenceNoSetting>(templateDto);

            _templateVariableSequenceNoSettingRepository.Add(template);
            if (_uow.Save() <= 0) throw new Exception("Creating template variable sequence failed on save.");
            return Ok(template.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] TemplateVariableSequenceNoSettingDto templateDto)
        {
            if (templateDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var template = _mapper.Map<TemplateVariableSequenceNoSetting>(templateDto);
            _templateVariableSequenceNoSettingRepository.Update(template);
            if (_uow.Save() <= 0) throw new Exception("Creating template variable sequence failed on save.");
            return Ok(template.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            //var record = _projectScheduleRepository.FindByInclude(x => x.Id == id, x => x.ProjectDesign)
            //    .FirstOrDefault();

            //var recordTemplate = _projectScheduleTemplateRepository.FindByInclude(x => x.ProjectScheduleId == id && x.DeletedDate == null).ToList();

            //if (record == null && recordTemplate == null)
            //    return NotFound();

            //if (!_studyVersionRepository.IsOnTrialByProjectDesing(record.ProjectDesign.Id))
            //{
            //    ModelState.AddModelError("Message", "Can not delete schedule!");
            //    return BadRequest(ModelState);
            //}

            //_projectScheduleRepository.Delete(record);
            //recordTemplate.ForEach(x =>
            //{
            //    _projectScheduleTemplateRepository.Delete(x);
            //});

            //_uow.Save();

            //_projectScheduleTemplateRepository.UpdateDesignTemplatesSchedule(record.ProjectDesignPeriodId);
            //_uow.Save();

            return Ok();
        }
    }
}

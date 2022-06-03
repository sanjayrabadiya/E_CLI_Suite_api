using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Entities.Project.Schedule;
using GSC.Respository.Project.Schedule;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.Project.Schedule
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleTerminateDetailsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IScheduleTerminateDetailsRepository _scheduleTerminateDetailsRepository;
        private readonly IUnitOfWork _uow;

        public ScheduleTerminateDetailsController(IMapper mapper, IUnitOfWork uow, IScheduleTerminateDetailsRepository scheduleTerminateDetailsRepository)
        {
            _mapper = mapper;
            _scheduleTerminateDetailsRepository = scheduleTerminateDetailsRepository;
            _uow = uow;
        }

        [HttpGet("GetData/{id}")]
        public IActionResult GetData(int id)
        {
            return Ok();
        }

        [HttpPost]
        public IActionResult Post([FromBody] ScheduleTerminateDetailsDto terminateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            terminateDto.Id = 0;
            var projectSchedule = _mapper.Map<ScheduleTerminateDetails>(terminateDto);

            _scheduleTerminateDetailsRepository.Add(projectSchedule);
            if (_uow.Save() <= 0) throw new Exception("Creating Schedule Terminate Details failed on save.");
            return Ok(projectSchedule.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ScheduleTerminateDetailsDto projectScheduleDto)
        {
            if (projectScheduleDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //var projectSchedule = _mapper.Map<ProjectSchedule>(projectScheduleDto);
            //_projectScheduleTemplateRepository.UpdateTemplates(projectSchedule);
            //_projectScheduleTemplateRepository.UpdateDesignTemplatesOrder(projectSchedule);

            //_projectScheduleRepository.Update(projectSchedule);
            //foreach (var item in projectSchedule.Templates)
            //{
            //    if (item.Id > 0)
            //        _projectScheduleTemplateRepository.Update(item);
            //    else
            //        _projectScheduleTemplateRepository.Add(item);
            //}
            //_uow.Save();

            //_projectScheduleTemplateRepository.UpdateDesignTemplatesSchedule(projectScheduleDto.ProjectDesignPeriodId);
            //_uow.Save();

            return Ok();
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

using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskTemplateController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ITaskTemplateRepository _taskTemplateRepository;
        public TaskTemplateController(
           IUnitOfWork uow, IMapper mapper,
           IJwtTokenAccesser jwtTokenAccesser,
           ITaskTemplateRepository taskTemplateRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _taskTemplateRepository = taskTemplateRepository;

        }
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var tasktemplate = _taskTemplateRepository.GetStudyTrackerList(isDeleted);
            return Ok(tasktemplate);
        }

        [HttpPost]
        public IActionResult Post([FromBody] TaskTemplateDto tasktemplateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            tasktemplateDto.Id = 0;
            var taskTemplate = _mapper.Map<TaskTemplate>(tasktemplateDto);
            var validate = _taskTemplateRepository.Duplicate(taskTemplate);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _taskTemplateRepository.Add(taskTemplate);
            if (_uow.Save() <= 0) throw new Exception("Creating Study Tracker template failed on save.");
            return Ok(taskTemplate.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] TaskTemplateDto tasktemplateDto)
        {
            if (tasktemplateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var taskTemplate = _mapper.Map<TaskTemplate>(tasktemplateDto);
            var validate = _taskTemplateRepository.Duplicate(taskTemplate);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _taskTemplateRepository.Update(taskTemplate);

            if (_uow.Save() <= 0) throw new Exception("Updating Study Tracker Template failed on save.");
            return Ok(taskTemplate.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _taskTemplateRepository.Find(id);

            if (record == null)
                return NotFound();

            _taskTemplateRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _taskTemplateRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _taskTemplateRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _taskTemplateRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetTaskTemplateDropDown")]
        public IActionResult GetTaskTemplateDropDown()
        {
            return Ok(_taskTemplateRepository.GetTaskTemplateDropDown());
        }
    }
}

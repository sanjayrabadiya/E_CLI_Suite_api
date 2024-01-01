using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class OverTimeMetricsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IOverTimeMetricsRepository _overTimeMetricsRepository;

        public OverTimeMetricsController(IUnitOfWork uow, IMapper mapper,
            IOverTimeMetricsRepository overTimeMasterRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _overTimeMetricsRepository = overTimeMasterRepository;
        }

        [HttpGet("{isDeleted:bool?}/{metricsId}/{projectId}/{countryId}/{siteId}")]
        public IActionResult Get(bool isDeleted, int metricsId, int projectId, int countryId, int siteId)
        {
            //Get ActualNo then Update from Randomization
            _overTimeMetricsRepository.UpdateAllActualNo(isDeleted, metricsId, projectId, countryId, siteId);
            var tasklist = _overTimeMetricsRepository.GetTasklist(isDeleted, metricsId, projectId, countryId, siteId);    
            return Ok(tasklist);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var task = _overTimeMetricsRepository.Find(id);
            var taskDto = _mapper.Map<OverTimeMetricsDto>(task);
            return Ok(taskDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] OverTimeMetricsDto overTimeMetricsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            overTimeMetricsDto.Id = overTimeMetricsDto.Id != 0 ? overTimeMetricsDto.Id : 0;
            var taskMaster = _mapper.Map<OverTimeMetrics>(overTimeMetricsDto);

            var PlanCheck = _overTimeMetricsRepository.PlannedCheck(taskMaster);
            if (!string.IsNullOrEmpty(PlanCheck))
            {
                ModelState.AddModelError("Message", PlanCheck);
                return BadRequest(ModelState);
            }

            taskMaster.Id = 0;
            taskMaster.If_Active = true;
            var updatePlanning = _overTimeMetricsRepository.UpdatePlanning(taskMaster);
            if (!string.IsNullOrEmpty(updatePlanning))
            {
                ModelState.AddModelError("Message", updatePlanning);
                return BadRequest(ModelState);
            }
            _overTimeMetricsRepository.Add(taskMaster);
            if (_uow.Save() <= 0) throw new Exception("Over Time failed on save.");

            return Ok(taskMaster.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] OverTimeMetricsDto overTimeMetricsDto)
        {
            var Id = overTimeMetricsDto.Id;
           var AddID = Post(overTimeMetricsDto);
            if(AddID!=null)
            {
                if (Id <= 0) return BadRequest();
                if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
                var task = _overTimeMetricsRepository.Find(Id);
                var taskmaster = _mapper.Map<OverTimeMetrics>(task);
                taskmaster.If_Active = false;
                _overTimeMetricsRepository.Update(taskmaster);
                if (_uow.Save() <= 0) throw new Exception("Updating Task Master failed on save.");
                return Ok(taskmaster.Id);
            }
            return Ok("");
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _overTimeMetricsRepository.Find(id);
            if (record == null)
                return NotFound();
            _overTimeMetricsRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _overTimeMetricsRepository.Find(id);
            if (record == null)
                return NotFound();
            //var validate = _overTimeMetricsRepository.Duplicate(record);
            //if (!string.IsNullOrEmpty(validate))
            //{
            //    ModelState.AddModelError("Message", validate);
            //    return BadRequest(ModelState);
            //}
            var PlanCheck = _overTimeMetricsRepository.PlannedCheck(record);
            if (!string.IsNullOrEmpty(PlanCheck))
            {
                ModelState.AddModelError("Message", PlanCheck);
                return BadRequest(ModelState);
            }
            _overTimeMetricsRepository.Active(record);
            _uow.Save();
            return Ok();
        }
        [HttpGet]
        [Route("GetChildProjectWithParentProjectDropDown/{parentProjectId}")]
        public IActionResult GetChildProjectWithParentProjectDropDown(int parentProjectId)
        {
            return Ok(_overTimeMetricsRepository.GetChildProjectWithParentProjectDropDown(parentProjectId));
        }

    }
}

using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsMonitoringController : BaseController
    {
        private readonly ICtmsMonitoringRepository _ctmsMonitoringRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CtmsMonitoringController(ICtmsMonitoringRepository ctmsMonitoringRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _ctmsMonitoringRepository = ctmsMonitoringRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var ctmsMonitoring = _ctmsMonitoringRepository.Find(id);
            var ctmsMonitoringDto = _mapper.Map<CtmsMonitoringDto>(ctmsMonitoring);
            return Ok(ctmsMonitoringDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] CtmsMonitoringDto ctmsMonitoringDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsMonitoringDto.Id = 0;
            var ctmsMonitoring = _mapper.Map<CtmsMonitoring>(ctmsMonitoringDto);
            _ctmsMonitoringRepository.Add(ctmsMonitoring);
            if (_uow.Save() <= 0) throw new Exception("Creating Monitoring failed on save.");

            return Ok(ctmsMonitoring.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CtmsMonitoringDto ctmsMonitoringDto)
        {
            if (ctmsMonitoringDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ctmsMonitoring = _mapper.Map<CtmsMonitoring>(ctmsMonitoringDto);

            _ctmsMonitoringRepository.Update(ctmsMonitoring);
            if (_uow.Save() <= 0) throw new Exception("Updating Monitoring failed on save.");
            return Ok(ctmsMonitoring.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _ctmsMonitoringRepository.Find(id);

            if (record == null)
                return NotFound();

            _ctmsMonitoringRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _ctmsMonitoringRepository.Find(id);

            if (record == null)
                return NotFound();
            _ctmsMonitoringRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        /// Get Monitoring Form list 
        /// Created By Swati
        [HttpGet]
        [Route("GetMonitoringForm/{projectId}/{siteId}/{activityId}")]
        public IActionResult GetMonitoringForm(int projectId, int siteId, int activityId)
        {
            if (projectId <= 0) return BadRequest();

            var result = _ctmsMonitoringRepository.GetMonitoringForm(projectId, siteId, activityId);
            return Ok(result);
        }

        [HttpGet]
        [Route("CloneForm/{ctmsMonitoringId}/{noOfClones}")]
        [TransactionRequired]
        public IActionResult CloneForm(int ctmsMonitoringId, int noOfClones)
        {
            if (ctmsMonitoringId <= 0)
            {
                ModelState.AddModelError("Message", "You can't able to clone the form as monitoring visit is not started!");
                return BadRequest(ModelState);
            }
            var CtmsMonitoringId = _ctmsMonitoringRepository.Find(ctmsMonitoringId).Id;

            for (var i = 1; i <= noOfClones; i++)
            {
                var monitoring = _ctmsMonitoringRepository.FindBy(t => t.Id == CtmsMonitoringId && t.DeletedDate == null).FirstOrDefault();
                monitoring.Id = 0;
                monitoring.ScheduleStartDate = null;
                monitoring.ScheduleEndDate = null;
                monitoring.ActualStartDate = null;
                monitoring.ActualEndDate = null;
                monitoring.ParentId = ctmsMonitoringId;
                monitoring.ModifiedBy = null;
                monitoring.ModifiedDate = null;
                _ctmsMonitoringRepository.Add(monitoring);
            }

            _uow.Save();

            return Ok();
        }
        [HttpGet]
        [Route("GetMonitoringFormforDashboard/{ctmsMonitoringId}/{activityId}")]
        public IActionResult GetMonitoringFormforDashboard(int ctmsMonitoringId, int activityId)
        {
            if (ctmsMonitoringId <= 0) return BadRequest();
            if (activityId <= 0) return BadRequest();

            var result = _ctmsMonitoringRepository.GetMonitoringFormforDashboard(ctmsMonitoringId, activityId);
            return Ok(result);
        }

        [HttpGet]
        [Route("AddMissed/{id}")]
        public ActionResult AddMissed(int id)
        {
            var record = _ctmsMonitoringRepository.Find(id);
            if (record == null)
                return NotFound();

            var ctmsMonitoring = _mapper.Map<CtmsMonitoring>(record);
            ctmsMonitoring.If_Missed = true;
            _ctmsMonitoringRepository.Update(ctmsMonitoring);
            if (_uow.Save() <= 0) throw new Exception("Updating Missed Monitoring failed on save.");

            return Ok();
        }
        [HttpGet]
        [Route("AddReSchedule/{id}")]
        public ActionResult AddReSchedule(int id)
        {
            var record = _ctmsMonitoringRepository.Find(id);
            if (record == null)
                return NotFound();

            var ctmsMonitoring = _mapper.Map<CtmsMonitoring>(record);
            ctmsMonitoring.If_ReSchedule = true;
            _ctmsMonitoringRepository.Update(ctmsMonitoring);
            if (_uow.Save() <= 0) throw new Exception("Updating Missed Monitoring failed on save.");

            record.Id = 0;
            var addCtmsMonitoring = _mapper.Map<CtmsMonitoring>(record);
            addCtmsMonitoring.ScheduleStartDate = null;
            addCtmsMonitoring.ScheduleEndDate = null;
            addCtmsMonitoring.ActualStartDate = null;
            addCtmsMonitoring.ActualEndDate = null;
            addCtmsMonitoring.ModifiedByUser = null;
            addCtmsMonitoring.ModifiedDate = null;
            addCtmsMonitoring.DeletedBy = null;
            addCtmsMonitoring.DeletedDate = null;
            addCtmsMonitoring.If_Missed = false;
            addCtmsMonitoring.If_ReSchedule = false;

            _ctmsMonitoringRepository.Add(addCtmsMonitoring);
            if (_uow.Save() <= 0) throw new Exception("Creating Monitoring failed on save.");

            return Ok();
        }
    }
}
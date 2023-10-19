using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class MetricsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IOverTimeMetricsRepository _overTimeMetricsRepository;
        private readonly IMetricsRepository _metricsRepository;
        public MetricsController(
           IUnitOfWork uow, IMapper mapper,
           IMetricsRepository MetricsRepository,
           IOverTimeMetricsRepository overTimeMasterRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _overTimeMetricsRepository = overTimeMasterRepository;
            _metricsRepository = MetricsRepository;
        }
        [HttpGet("{isDeleted:bool?}/{typesId}")]
        public IActionResult Get(bool isDeleted, int typesId)
        {
            var planMetrics = _metricsRepository.GetMetricsList(isDeleted, typesId);
            return Ok(planMetrics);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var planMetrics = _metricsRepository.Find(id);
            var planMetricsdetail = _mapper.Map<PlanMetrics>(planMetrics);
            return Ok(planMetricsdetail);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] PlanMetricsDto planMetricsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            planMetricsDto.Id = 0;
            var planMetrics = _mapper.Map<PlanMetrics>(planMetricsDto);
            var validate = _metricsRepository.Duplicate(planMetrics);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _metricsRepository.Add(planMetrics);
            if (_uow.Save() <= 0) throw new Exception("Creating Plan Metrics failed on save.");
            return Ok(planMetrics.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PlanMetricsDto planMetricsDto)
        {
            if (planMetricsDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var metricsPlan = _mapper.Map<PlanMetrics>(planMetricsDto);
            var validate = _metricsRepository.Duplicate(metricsPlan);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _metricsRepository.Update(metricsPlan);
            if (_uow.Save() <= 0) throw new Exception("Plan Metrics is failed on save.");
            return Ok(metricsPlan.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _metricsRepository.Find(id);
            var parenttask = _overTimeMetricsRepository.FindBy(x => x.PlanMetricsId == id);
            foreach (var task in parenttask)
            {
                if (record == null)
                    return NotFound();
                _overTimeMetricsRepository.Delete(task);
            }
            if (record == null)
                return NotFound();

            _metricsRepository.Delete(record);
            _uow.Save();

            return Ok();
        }
        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _metricsRepository.Find(id);
            var parenttask = _overTimeMetricsRepository.FindBy(x => x.PlanMetricsId == id);
            foreach (var task in parenttask)
            {
                if (record == null)
                    return NotFound();
                _overTimeMetricsRepository.Active(task);
            }
            if (record == null)
                return NotFound();

            var validate = _metricsRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _metricsRepository.Active(record);
            _uow.Save();
            return Ok();
        }
    }
}

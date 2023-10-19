using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class CtmsMonitoringStatusController : BaseController
    {
        private readonly ICtmsMonitoringStatusRepository _ctmsMonitoringStatusRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CtmsMonitoringStatusController(ICtmsMonitoringStatusRepository ctmsMonitoringStatusRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _ctmsMonitoringStatusRepository = ctmsMonitoringStatusRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetCtmsMonitoringStatusList/{CtmsMonitoringId}")]
        public IActionResult GetCtmsMonitoringStatusList(int CtmsMonitoringId)
        {
            if (CtmsMonitoringId <= 0) return BadRequest();
            var ctmsActionPoint = _ctmsMonitoringStatusRepository.GetCtmsMonitoringStatusList(CtmsMonitoringId);
            return Ok(ctmsActionPoint);
        }

        [HttpGet]
        [Route("GetSiteStatus/{ProjectId}")]
        public IActionResult GetSiteStatus(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();
            var status = _ctmsMonitoringStatusRepository.GetSiteStatus(ProjectId);
            return Ok(status);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CtmsMonitoringStatusDto ctmsMonitoringStatusDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsMonitoringStatusDto.Id = 0;
            var ctmsMonitoringStatus = _mapper.Map<CtmsMonitoringStatus>(ctmsMonitoringStatusDto);
            _ctmsMonitoringStatusRepository.Add(ctmsMonitoringStatus);
            if (_uow.Save() <= 0) throw new Exception("Creating status failed on save.");

            return Ok(ctmsMonitoringStatus.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CtmsMonitoringStatusDto ctmsMonitoringStatusDto)
        {
            if (ctmsMonitoringStatusDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ctmsMonitoringStatus = _mapper.Map<CtmsMonitoringStatus>(ctmsMonitoringStatusDto);

            _ctmsMonitoringStatusRepository.Update(ctmsMonitoringStatus);
            if (_uow.Save() <= 0) throw new Exception("Updating status failed on save.");
            return Ok(ctmsMonitoringStatus.Id);
        }

        [HttpGet]
        [Route("GetFormApprovedOrNot/{projectId}/{siteId}/{tabNumber}")]
        public IActionResult GetFormApprovedOrNot(int projectId, int siteId, int tabNumber)
        {
            if (projectId <= 0) return BadRequest();

            var result = _ctmsMonitoringStatusRepository.GetFormApprovedOrNot(projectId, siteId, tabNumber);
            if (!string.IsNullOrEmpty(result))
            {
                ModelState.AddModelError("Message", result);
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}
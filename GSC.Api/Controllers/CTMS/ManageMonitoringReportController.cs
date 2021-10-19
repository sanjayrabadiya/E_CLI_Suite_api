using System;
using System.Collections.Generic;
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

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ManageMonitoringReportController : BaseController
    {
        private readonly IManageMonitoringReportRepository _manageMonitoringReportRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ManageMonitoringReportController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IManageMonitoringReportRepository manageMonitoringReportRepository)
        {
            _manageMonitoringReportRepository = manageMonitoringReportRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetMonitoringReport/{projectId}")]
        public IActionResult GetMonitoringReport(int projectId)
        {
            if (projectId <= 0) return BadRequest();

            var result = _manageMonitoringReportRepository.GetMonitoringReport(projectId);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ManageMonitoringReportDto manageMonitoringReportDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            manageMonitoringReportDto.Id = 0;
            var manageMonitoringReport = _mapper.Map<ManageMonitoringReport>(manageMonitoringReportDto);
            
            _manageMonitoringReportRepository.Add(manageMonitoringReport);
            if (_uow.Save() <= 0) throw new Exception("Creating Report failed on save.");
            return Ok(manageMonitoringReport.Id);
        }
    }
}
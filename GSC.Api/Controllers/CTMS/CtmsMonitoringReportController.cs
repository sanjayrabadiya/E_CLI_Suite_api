using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class CtmsMonitoringReportController : BaseController
    {
        private readonly ICtmsMonitoringReportRepository _ctmsMonitoringReportRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CtmsMonitoringReportController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            ICtmsMonitoringReportRepository ctmsMonitoringReportRepository)
        {
            _ctmsMonitoringReportRepository = ctmsMonitoringReportRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        /// Add ctms monitoring report
        /// Created By Swati
        [HttpPost]
        public IActionResult Post([FromBody] CtmsMonitoringReportDto ctmsMonitoringReportDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsMonitoringReportDto.Id = 0;
            ctmsMonitoringReportDto.ReportStatus = MonitoringReportStatus.NotInitiated;
            var ctmsMonitoringReport = _mapper.Map<CtmsMonitoringReport>(ctmsMonitoringReportDto);

            _ctmsMonitoringReportRepository.Add(ctmsMonitoringReport);
            if (_uow.Save() <= 0) throw new Exception("Creating Report failed on save.");
            return Ok(ctmsMonitoringReport.Id);
        }
    }
}
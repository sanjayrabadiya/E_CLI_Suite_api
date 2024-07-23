using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class CtmsMonitoringReportController : BaseController
    {
        private readonly ICtmsMonitoringReportRepository _ctmsMonitoringReportRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CtmsMonitoringReportController(IUnitOfWork uow, IMapper mapper,
            ICtmsMonitoringReportRepository ctmsMonitoringReportRepository)
        {
            _ctmsMonitoringReportRepository = ctmsMonitoringReportRepository;
            _uow = uow;
            _mapper = mapper;
        }

        /// Add ctms monitoring report
        /// Created By Swati
        [HttpPost]
        public IActionResult Post([FromBody] CtmsMonitoringReportDto ctmsMonitoringReportDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ctmsMonitoringReportDto.Id = 0;
            //Changes made by Sachin
            ctmsMonitoringReportDto.ReportStatus = MonitoringReportStatus.OnGoing;
            var ctmsMonitoringReport = _mapper.Map<CtmsMonitoringReport>(ctmsMonitoringReportDto);

            _ctmsMonitoringReportRepository.Add(ctmsMonitoringReport);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating Report failed on save."));
            return Ok(ctmsMonitoringReport.Id);
        }

        [HttpGet]
        [Route("GetMonitoringFormApprovedOrNOt/{projectId}/{siteId}/{tabNumber}")]
        public IActionResult GetMonitoringFormApprovedOrNOt(int projectId, int siteId, int tabNumber)
        {
            if (projectId <= 0) return BadRequest();

            var result = _ctmsMonitoringReportRepository.GetMonitoringFormApprovedOrNot(projectId, siteId, tabNumber);
            if (!string.IsNullOrEmpty(result))
            {
                ModelState.AddModelError("Message", result);
                return BadRequest(ModelState);
            }

            return Ok();
        }
    }
}
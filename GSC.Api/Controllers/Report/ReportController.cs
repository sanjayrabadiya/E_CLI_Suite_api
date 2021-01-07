using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Entities.Custom;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.Workflow;
using GSC.Data.Entities.Report;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Report;
using GSC.Respository.Project.Design;
using GSC.Respository.Reports;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class ReportController : BaseController
    {
        private readonly IGscReport _gscReport;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork _uow;
        private readonly IProjectDesignReportSettingRepository _projectDesignReportSettingRepository;
        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IMapper _mapper;
        private readonly IReportSyncfusion _reportSuncfusion;
        public ReportController(IProjectDesignReportSettingRepository projectDesignReportSettingRepository, IGscReport gscReport
            , IUnitOfWork uow, IJwtTokenAccesser jwtTokenAccesser, IJobMonitoringRepository jobMonitoringRepository,
            IProjectDesignRepository projectDesignRepository,
            IMapper mapper, IReportSyncfusion reportSuncfusion)
        {
            _uow = uow;
            _gscReport = gscReport;
            _projectDesignReportSettingRepository = projectDesignReportSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _jobMonitoringRepository = jobMonitoringRepository;
            _mapper = mapper;
            _projectDesignRepository = projectDesignRepository;
            _reportSuncfusion = reportSuncfusion;
        }

        [HttpGet]
        [Route("GetProjectDesign/{projectDesignId}")]
        [AllowAnonymous]
        public IActionResult GetProjectDesign(int projectDesignId)
        {
            var abc = _gscReport.GetProjectDesign(projectDesignId);
            return abc;
        }

        //[HttpGet]
        //[Route("GetProjectDesignWithFliter/{projectDesignId}/{periodId}/{visitId}/{templateId}/{annotation}/{pdfType}/{pdfStatus}/{reportSetting}")]
        //[AllowAnonymous]
        //public IActionResult GetProjectDesignWithFliter(int projectDesignId, int[] periodId, int[] visitId, int[] templateId, bool annotation, int pdfType, int pdfStatus, [FromRoute]ReportSettingDto reportSetting)
        //{
        //    var abc = _gscReport.GetProjectDesignWithFliter(projectDesignId, periodId, visitId, templateId, null, annotation, pdfType, pdfStatus);
        //    return abc;
        //}

        [HttpPost]
        [Route("GetProjectDesignWithFliter")]
        public async Task<IActionResult> GetProjectDesignWithFliter([FromBody] ReportSettingNew reportSetting)
        {
            //var projectdesign = _projectDesignRepository.FindBy(x => x.ProjectId == reportSetting.ProjectId).SingleOrDefault();
           
            JobMonitoringDto jobMonitoringDto = new JobMonitoringDto();
            jobMonitoringDto.JobName = JobNameType.DossierReport;
            jobMonitoringDto.JobDescription = reportSetting.ProjectId;
            jobMonitoringDto.JobType = JobTypeEnum.Report;
            jobMonitoringDto.JobStatus = JobStatusType.InProcess;
            jobMonitoringDto.SubmittedBy = _jwtTokenAccesser.UserId;
            jobMonitoringDto.SubmittedTime = DateTime.Now.UtcDateTime();
            jobMonitoringDto.JobDetails = (DossierPdfStatus)reportSetting.PdfStatus;
            var jobMonitoring = _mapper.Map<JobMonitoring>(jobMonitoringDto);
            _jobMonitoringRepository.Add(jobMonitoring);

            if (_uow.Save() <= 0) throw new Exception("Creating Job Monitoring failed on save.");

            _reportSuncfusion.BlankReportGenerate(reportSetting, jobMonitoring);

            //return result;
           // return result.FileDownloadName.ToString();
             return Ok();
        }
    }
}
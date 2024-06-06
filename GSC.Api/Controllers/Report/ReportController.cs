using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Report;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Report;
using GSC.Respository.Project.Design;
using GSC.Respository.Reports;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.Report
{
    [Route("api/[controller]")]
    public class ReportController : BaseController
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IProjectDesignReportSettingRepository _projectDesignReportSettingRepository;
        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        private readonly IMapper _mapper;
        private readonly IReportSyncfusion _reportSuncfusion;
        public ReportController(IProjectDesignReportSettingRepository projectDesignReportSettingRepository
            , IUnitOfWork uow, IJwtTokenAccesser jwtTokenAccesser, IJobMonitoringRepository jobMonitoringRepository,
            IMapper mapper, IReportSyncfusion reportSuncfusion, IGSCContext context)
        {
            _uow = uow;
            _projectDesignReportSettingRepository = projectDesignReportSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _jobMonitoringRepository = jobMonitoringRepository;
            _mapper = mapper;
            _reportSuncfusion = reportSuncfusion;
            _context = context;
        }

        [HttpGet]
        [Route("GetProjectDesign/{projectDesignId}")]
        public IActionResult GetProjectDesign(int projectDesignId)
        {

            ReportSettingNew reportSetting = new ReportSettingNew();
            reportSetting.ProjectId = projectDesignId;
            reportSetting.PdfStatus = DossierPdfStatus.Blank;
            reportSetting.AnnotationType = true;
            reportSetting.LeftMargin = Convert.ToDecimal(0.5);
            reportSetting.RightMargin = Convert.ToDecimal(0.5);
            reportSetting.BottomMargin = Convert.ToDecimal(0.5);
            reportSetting.TopMargin = Convert.ToDecimal(0.5);
            reportSetting.IsClientLogo = true;
            reportSetting.IsCompanyLogo = true;
            reportSetting.NonCRF = true;

            var abc = _reportSuncfusion.GetProjectDesign(reportSetting);
            return abc;
        }

        [TransactionRequired]
        [HttpPost]
        [Route("GetScreeningWithFliter")]
        public async Task<IActionResult> GetScreeningWithFliter([FromBody] ScreeningReportSetting reportSetting)
        {

            JobMonitoringDto jobMonitoringDto = new JobMonitoringDto();
            jobMonitoringDto.JobName = JobNameType.ScreeningReport;
            jobMonitoringDto.JobDescription = reportSetting.ProjectId;
            jobMonitoringDto.JobType = JobTypeEnum.Report;
            jobMonitoringDto.JobStatus = JobStatusType.InProcess;
            jobMonitoringDto.SubmittedBy = _jwtTokenAccesser.UserId;
            jobMonitoringDto.SubmittedTime = _jwtTokenAccesser.GetClientDate();
            jobMonitoringDto.JobDetails = reportSetting.PdfStatus == DossierPdfStatus.Blank ? DossierPdfStatus.Blank : DossierPdfStatus.Subject;
            var jobMonitoring = _mapper.Map<JobMonitoring>(jobMonitoringDto);

            _jobMonitoringRepository.Add(jobMonitoring);

            if (_uow.Save() <= 0) return Ok(new Exception("Creating Job Monitoring failed on save."));
            string message = _reportSuncfusion.ScreeningPdfReportGenerate(reportSetting, jobMonitoring);

            if (!string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("Message", message);
                return BadRequest(ModelState);
            }
            return Ok();
        }


        [HttpPost]
        [Route("GetProjectDesignWithFliter")]
        public async Task<IActionResult> GetProjectDesignWithFliter([FromBody] ReportSettingNew reportSetting)
        {
            #region Report Setting Save
            var reportSettingForm = _projectDesignReportSettingRepository.All.Where(x => x.ProjectDesignId == reportSetting.ProjectId && x.CompanyId == reportSetting.CompanyId && x.DeletedBy == null).FirstOrDefault();
            var objNew = _mapper.Map<ProjectDesignReportSetting>(reportSetting);
            if (reportSettingForm == null)
            {
                _projectDesignReportSettingRepository.Add(objNew);
            }
            else
            {
                objNew.Id = reportSettingForm.Id;
                _projectDesignReportSettingRepository.Update(objNew);
            }
            #endregion
            JobMonitoringDto jobMonitoringDto = new JobMonitoringDto();
            jobMonitoringDto.JobName = JobNameType.DossierReport;
            jobMonitoringDto.JobDescription = reportSetting.ProjectId;
            jobMonitoringDto.JobType = JobTypeEnum.Report;
            jobMonitoringDto.JobStatus = JobStatusType.InProcess;
            jobMonitoringDto.SubmittedBy = _jwtTokenAccesser.UserId;
            jobMonitoringDto.SubmittedTime = _jwtTokenAccesser.GetClientDate();
            jobMonitoringDto.JobDetails = reportSetting.PdfStatus;
            var jobMonitoring = _mapper.Map<JobMonitoring>(jobMonitoringDto);
            if (!reportSetting.IsSync)
                _jobMonitoringRepository.Add(jobMonitoring);

            if (_uow.Save() <= 0) return Ok(new Exception("Creating Job Monitoring failed on save."));
            string message = await _reportSuncfusion.DossierPdfReportGenerate(reportSetting, jobMonitoring);

            if (!string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("Message", message);
                return BadRequest(ModelState);
            }
            return Ok();
        }


        [HttpGet]
        [Route("GetVisitsByCrfTypes/{CRFType?}/{projectId?}")]
        public IActionResult GetVisitsByCrfTypes(int? CRFType, int? projectId)
        {
            if (projectId <= 0 || CRFType <= 0)
            {
                return null;
            }
            var Data = _context.ProjectDesignVisit.Where(a => a.DeletedDate == null
                      && a.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
                     && (CRFType == 3 || ((CRFType == 1 && a.IsNonCRF) || (CRFType == 2 && !a.IsNonCRF))))
                    .Select(x => new DropDownDto
                    {
                        Id = x.Id,
                        Value = x.DisplayName
                    }).Distinct().ToList();


            return Ok(Data);
        }

        [HttpPost]
        [Route("GetTemplatesByVisits")]
        public IActionResult GetTemplatesByVisits([FromBody] ReportVisitsDto reportSetting)
        {
            if (reportSetting.VisitIds == null)
            {
                return null;
            }
            var Data = _context.ProjectDesignTemplate.Where(a => a.DeletedDate == null && reportSetting.VisitIds.Contains(a.ProjectDesignVisitId))
                    .Select(x => new DropDownDto
                    {
                        Id = x.Id,
                        Value = x.TemplateName
                    }).Distinct().ToList();


            return Ok(Data);
        }
    }
}
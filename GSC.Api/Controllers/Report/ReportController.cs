﻿using AutoMapper;
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
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IProjectDesignReportSettingRepository _projectDesignReportSettingRepository;
        private readonly IJobMonitoringRepository _jobMonitoringRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IMapper _mapper;
        public ReportController(IProjectDesignReportSettingRepository projectDesignReportSettingRepository, IGscReport gscReport
            , IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IJobMonitoringRepository jobMonitoringRepository,
            IProjectDesignRepository projectDesignRepository,
            IMapper mapper)
        {
            _uow = uow;
            _gscReport = gscReport;
            _projectDesignReportSettingRepository = projectDesignReportSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _jobMonitoringRepository = jobMonitoringRepository;
            _mapper = mapper;
            _projectDesignRepository = projectDesignRepository;
        }

        [HttpGet]
        [Route("GetProjectDesign/{projectDesignId}/{periodId}/{visitId}/{templateId}/{annotation}")]
        [AllowAnonymous]
        public IActionResult GetProjectDesign(int projectDesignId, int periodId, int visitId, int templateId, bool annotation)
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
        //public IActionResult GetProjectDesignWithFliter([FromBody]ReportSettingNew reportSetting)
        public async Task<string> GetProjectDesignWithFliter([FromBody] ReportSettingNew reportSetting)
        {
            var projectId = _projectDesignRepository.Find(reportSetting.ProjectId).ProjectId;
            #region Report Setting Save
            var reportSettingForm = _projectDesignReportSettingRepository.All.Where(x => x.ProjectDesignId == reportSetting.ProjectId && x.CompanyId == reportSetting.CompanyId && x.DeletedBy == null).FirstOrDefault();
            if (reportSettingForm == null)
            {
                ProjectDesignReportSetting objNew = new ProjectDesignReportSetting();

                objNew.ProjectDesignId = reportSetting.ProjectId;
                objNew.IsClientLogo = reportSetting.IsClientLogo;
                objNew.IsCompanyLogo = reportSetting.IsCompanyLogo;
                objNew.IsInitial = reportSetting.IsInitial;
                objNew.IsScreenNumber = reportSetting.IsScreenNumber;
                objNew.IsSponsorNumber = reportSetting.IsSponsorNumber;
                objNew.IsSubjectNumber = reportSetting.IsSubjectNumber;
                objNew.LeftMargin = reportSetting.LeftMargin;
                objNew.RightMargin = reportSetting.RightMargin;
                objNew.TopMargin = reportSetting.TopMargin;
                objNew.BottomMargin = reportSetting.BottomMargin;
                _projectDesignReportSettingRepository.Add(objNew);
            }
            else
            {
                reportSettingForm.ProjectDesignId = reportSetting.ProjectId;
                reportSettingForm.IsClientLogo = reportSetting.IsClientLogo;
                reportSettingForm.IsCompanyLogo = reportSetting.IsCompanyLogo;
                reportSettingForm.IsInitial = reportSetting.IsInitial;
                reportSettingForm.IsScreenNumber = reportSetting.IsScreenNumber;
                reportSettingForm.IsSponsorNumber = reportSetting.IsSponsorNumber;
                reportSettingForm.IsSubjectNumber = reportSetting.IsSubjectNumber;
                reportSettingForm.LeftMargin = reportSetting.LeftMargin;
                reportSettingForm.RightMargin = reportSetting.RightMargin;
                reportSettingForm.TopMargin = reportSetting.TopMargin;
                reportSettingForm.BottomMargin = reportSetting.BottomMargin;

                _projectDesignReportSettingRepository.Update(reportSettingForm);
            }
            if (_uow.Context.SaveChanges(_jwtTokenAccesser) <= 0)
            {
                throw new Exception($"Creating Report Setting failed on save.");
            }
            #endregion

            #region Job Monitoring Save - Inprocess Status
            JobMonitoringDto jobMonitoringDto = new JobMonitoringDto();
            jobMonitoringDto.JobName = JobNameType.DossierReport;
            jobMonitoringDto.JobDescription = projectId;
            jobMonitoringDto.JobType = JobTypeEnum.Report;
            jobMonitoringDto.JobStatus = JobStatusType.InProcess;
            jobMonitoringDto.SubmittedBy = _jwtTokenAccesser.UserId;
            jobMonitoringDto.SubmittedTime = DateTime.Now.UtcDateTime();
            jobMonitoringDto.JobDetails = (DossierPdfStatus)reportSetting.PdfStatus;
            var jobMonitoring = _mapper.Map<JobMonitoring>(jobMonitoringDto);
            _jobMonitoringRepository.Add(jobMonitoring);

            if (_uow.Save() <= 0) throw new Exception("Creating Job Monitoring failed on save.");
            #endregion

            #region Get Data for Company
            var sqlquery = _projectDesignReportSettingRepository.GetProjectDesignWithFliter(reportSetting);
            #endregion

            #region Print Report
            var result = _gscReport.GetProjectDesignWithFliter(reportSetting, sqlquery.FirstOrDefault(), jobMonitoring);
            #endregion

            return result.FileDownloadName.ToString();
        }
    }
}
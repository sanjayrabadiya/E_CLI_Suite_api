using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Respository.AdverseEvent;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Respository.Etmf;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class DashboardController : BaseController
    {
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IProjectSubSecArtificateDocumentApproverRepository _projectSubSecArtificateDocumentApproverRepository;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IProjectSubSecArtificateDocumentReviewRepository _projectSubSecArtificateDocumentReviewRepository;
        private readonly IManageMonitoringReportReviewRepository _manageMonitoringReportReviewRepository;
        private readonly IAEReportingRepository _aEReportingRepository;

        public DashboardController(
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IProjectSubSecArtificateDocumentApproverRepository projectSubSecArtificateDocumentApproverRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository,
            IManageMonitoringReportReviewRepository manageMonitoringReportReviewRepository,
            IAEReportingRepository aEReportingRepository
            )
        {
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _projectSubSecArtificateDocumentApproverRepository = projectSubSecArtificateDocumentApproverRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
            _manageMonitoringReportReviewRepository = manageMonitoringReportReviewRepository;
            _aEReportingRepository = aEReportingRepository;
        }

        [HttpGet]
        [Route("GetMyTaskList/{ProjectId}/{SiteId:int?}")]
        public IActionResult GetMyTaskList(int ProjectId, int? SiteId)
        {
            DashboardDetailsDto objdashboard = new DashboardDetailsDto();
            objdashboard.eTMFApproveData = _projectArtificateDocumentApproverRepository.GetEtmfMyTaskList(ProjectId);
            objdashboard.eTMFSendData = _projectWorkplaceArtificateDocumentReviewRepository.GetSendDocumentList(ProjectId);
            objdashboard.eTMFSubSecApproveData = _projectSubSecArtificateDocumentApproverRepository.GetEtmfMyTaskList(ProjectId);
            objdashboard.eTMFSubSecSendData = _projectSubSecArtificateDocumentReviewRepository.GetSendDocumentList(ProjectId);
            objdashboard.eTMFSendBackData = _projectWorkplaceArtificateDocumentReviewRepository.GetSendBackDocumentList(ProjectId);
            objdashboard.eTMFSubSecSendBackData = _projectSubSecArtificateDocumentReviewRepository.GetSendBackDocumentList(ProjectId);
            objdashboard.eConsentData = _econsentReviewDetailsRepository.GetEconsentMyTaskList(ProjectId);
            objdashboard.eAdverseEventData = _aEReportingRepository.GetAEReportingMyTaskList(ProjectId);
            objdashboard.manageMonitoringReportSendData = _manageMonitoringReportReviewRepository.GetSendTemplateList((int)(SiteId != null ? SiteId : ProjectId));
            objdashboard.manageMonitoringReportSendBackData = _manageMonitoringReportReviewRepository.GetSendBackTemplateList((int)(SiteId != null ? SiteId : ProjectId));
            return Ok(objdashboard);
        }
    }
}
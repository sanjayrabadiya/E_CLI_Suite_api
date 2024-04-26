using System;
using GSC.Api.Controllers.Common;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.Attendance;
using GSC.Respository.Etmf;
using GSC.Respository.LogReport;
using GSC.Respository.Master;
using GSC.Respository.SupplyManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class QuartzJobController : BaseController
    {

        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IProjectSubSecArtificateDocumentReviewRepository _projectSubSecArtificateDocumentReviewRepository;
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IProjectSubSecArtificateDocumentApproverRepository _projectSubSecArtificateDocumentApproverRepository;
        private readonly IUserLoginReportRespository _userLoginReportRepository;
        private readonly IGSCContext _context;
        private readonly ISupplyManagementRequestRepository _supplyManagementRequestRepository;
        private readonly ISupplyManagementShipmentRepository _supplyManagementShipmentRepository;
        private readonly IVerificationApprovalTemplateRepository _verificationApprovalTemplateRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IResourceMilestoneRepository _resourceMilestoneRepository;
        public QuartzJobController(IUserLoginReportRespository userLoginReportRepository, IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository,
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IProjectSubSecArtificateDocumentApproverRepository projectSubSecArtificateDocumentApproverRepository,
            ISupplyManagementRequestRepository supplyManagementRequestRepository,
            ISupplyManagementShipmentRepository supplyManagementShipmentRepository,
            IGSCContext context,
            IVerificationApprovalTemplateRepository verificationApprovalTemplateRepository,
            IRandomizationRepository randomizationRepository,
            IResourceMilestoneRepository resourceMilestoneRepository)
        {
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _projectSubSecArtificateDocumentApproverRepository = projectSubSecArtificateDocumentApproverRepository;
            _userLoginReportRepository = userLoginReportRepository;
            _supplyManagementRequestRepository = supplyManagementRequestRepository;
            _supplyManagementShipmentRepository = supplyManagementShipmentRepository;
            _context = context;
            _verificationApprovalTemplateRepository = verificationApprovalTemplateRepository;
            _randomizationRepository = randomizationRepository;
            _resourceMilestoneRepository = resourceMilestoneRepository;

        }

        [HttpPost]
        [Route("ETMFJob")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> ETMFJob([FromBody] ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess response = new ProjectRemoveDataSuccess();
            try
            {
                _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
                await _projectWorkplaceArtificateDocumentReviewRepository.SendDueReviewEmail();
                await _projectSubSecArtificateDocumentReviewRepository.SendDueReviewEmail();
                await _projectArtificateDocumentApproverRepository.SendDueApproveEmail();
                await _projectSubSecArtificateDocumentApproverRepository.SendDueApproveEmail();
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                Log.Error("Error in Scheduler ETMF ", ex);
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("IWRSJob")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> IWRSJob([FromBody] ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess response = new ProjectRemoveDataSuccess();
            try
            {
                _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
                await _supplyManagementRequestRepository.ShipmentRequestEmailSchedule();
                await _supplyManagementShipmentRepository.ShipmentShipmentEmailSchedule();
                await _verificationApprovalTemplateRepository.SendForApprovalVerificationTemplateScheduleEmail();
                await _randomizationRepository.SendRandomizationThresholdEmailSchedule();
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                SupplyManagementEmailScheduleLog supplyManagementEmailScheduleLog = new SupplyManagementEmailScheduleLog();
                supplyManagementEmailScheduleLog.Message = ex.Message.ToString();
                supplyManagementEmailScheduleLog.TriggerType = "Error In IWRS Email Schedule Log";
                _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog);
                _context.Save();
                Log.Error("Error in Scheduler IWRS ", ex);
            }
            return Ok();
        }

        [HttpPost]
        [Route("CTMSJob")]
        [AllowAnonymous]
        public async System.Threading.Tasks.Task<IActionResult> CTMSJob([FromBody] ProjectRemoveDataDto obj)
        {
            ProjectRemoveDataSuccess response = new ProjectRemoveDataSuccess();
            try
            {
                _userLoginReportRepository.SetDbConnection(obj.ConnectionString);
                await _resourceMilestoneRepository.SendDueResourceMilestoneEmail();
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                Log.Error("Error in Scheduler CTMS ", ex);
            }
            return Ok(response);
        }
    }
}
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EmailSender;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementShipmentRepository : GenericRespository<SupplyManagementShipment>, ISupplyManagementShipmentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private static Random random = new Random();
        private readonly ISupplyManagementRequestRepository _supplyManagementRequestRepository;
        private readonly IUnitOfWork _uow;
        private readonly IEmailSenderRespository _emailSenderRespository;
        public SupplyManagementShipmentRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, ISupplyManagementRequestRepository supplyManagementRequestRepository,
            IEmailSenderRespository emailSenderRespository,
            IUnitOfWork uow)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _supplyManagementRequestRepository = supplyManagementRequestRepository;
            _emailSenderRespository = emailSenderRespository;
            _uow = uow;
        }
        public List<SupplyManagementShipmentGridDto> GetSupplyShipmentList(int parentProjectId, int SiteId, bool isDeleted)
        {
            List<SupplyManagementShipmentGridDto> FinalData = new List<SupplyManagementShipmentGridDto>();

            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                        Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == parentProjectId
                        && s.RoleId == _jwtTokenAccesser.RoleId);

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == parentProjectId).FirstOrDefault();

            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && ((x.SupplyManagementRequest.IsSiteRequest == true && x.SupplyManagementRequest.ToProjectId == SiteId)
            || (x.SupplyManagementRequest.IsSiteRequest == false && x.SupplyManagementRequest.FromProjectId == SiteId))).
                    ProjectTo<SupplyManagementShipmentGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            data.ForEach(t =>
            {
                t.StudyProductTypeName = setting != null && setting.IsBlindedStudy == true && isShow ? "" : t.StudyProductTypeName;
            });

            var requestdata = _context.SupplyManagementRequest.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null
                       && ((x.IsSiteRequest == true && x.ToProjectId == SiteId)
            || (x.IsSiteRequest == false && x.FromProjectId == SiteId)) && !data.Select(x => x.SupplyManagementRequestId).Contains(x.Id)).
                     ProjectTo<SupplyManagementShipmentGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            requestdata.ForEach(t =>
            {
                SupplyManagementShipmentGridDto obj = new SupplyManagementShipmentGridDto();
                obj = t;
                obj.Id = 0;
                obj.RequestDate = t.CreatedDate;
                obj.CreatedByUser = null;
                obj.CreatedDate = null;
                obj.ProductUnitType = t.StudyProductTypeId > 0 ? t.ProductUnitType : ProductUnitType.Kit;
                var product = t.StudyProductTypeId > 0 ? _context.PharmacyStudyProductType.Include(x => x.ProductType).Where(x => x.Id == t.StudyProductTypeId).Select(x => x.ProductType.ProductTypeName).FirstOrDefault() : "";
                obj.StudyProductTypeName = setting != null && setting.IsBlindedStudy == true && isShow ? "" : product;
                if (t.StudyProductTypeId > 0)
                {
                    var ptype = _context.PharmacyStudyProductType.Where(x => x.Id == t.StudyProductTypeId).FirstOrDefault();
                    if (ptype != null)
                    {
                        obj.StudyProductTypeUnitName = ptype.ProductUnitType.GetDescription();
                    }
                }
                else
                {
                    obj.StudyProductTypeUnitName = "Kit";
                }

                var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                if (fromproject != null)
                {
                    var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                    obj.StudyProjectCode = study != null ? study.ProjectCode : "";
                    obj.ProjectId = study.Id;
                }
                t.SiteRequest = t.IsSiteRequest ? "Site to Site" : "Site to Study";

                var workflowdetail = _context.SupplyManagementApprovalDetails.Include(a => a.SupplyManagementApproval)
               .Where(s => s.DeletedDate == null && s.SupplyManagementApproval.ProjectId == parentProjectId && s.SupplyManagementApproval.ApprovalType == SupplyManagementApprovalType.ShipmentApproval).ToList();
                if (workflowdetail.Count > 0)
                {
                    var workflow = workflowdetail.Where(s => s.UserId == _jwtTokenAccesser.UserId && s.SupplyManagementApproval.RoleId == _jwtTokenAccesser.RoleId).FirstOrDefault();

                    var supplyManagementShipmentApproval = _context.SupplyManagementShipmentApproval.Include(a => a.Users).Where(z => z.SupplyManagementRequestId == t.SupplyManagementRequestId).FirstOrDefault();

                    if (workflow != null && supplyManagementShipmentApproval == null)
                    {
                        obj.IsWorkflowApproval = true;
                    }
                    if (supplyManagementShipmentApproval != null)
                    {
                        obj.WorkflowApprovalName = supplyManagementShipmentApproval.Users.UserName;
                        obj.WorkflowApprovalRoleName = _context.SecurityRole.Where(a => a.Id == supplyManagementShipmentApproval.RoleId).FirstOrDefault().RoleName;
                        obj.WorkflowDate = supplyManagementShipmentApproval.CreatedDate;
                        obj.WorkflowId = supplyManagementShipmentApproval.Id;
                        obj.WorkflowComments = supplyManagementShipmentApproval.Comments;
                        obj.WorkflowStatus = supplyManagementShipmentApproval.Status;
                        obj.IsWorkflowApproval = false;
                    }
                }
                else
                {
                    obj.WorkflowStatus = SupplyManagementApprovalStatus.Approved;
                }
                obj.IpAddress = t.IpAddress;
                obj.TimeZone = t.TimeZone;
                FinalData.Add(obj);

            });
            return FinalData.OrderByDescending(x => x.RequestDate).ToList();
        }

        public string GenerateShipmentNo()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public string ApprovalValidation(SupplyManagementShipmentDto supplyManagementshipmentDto)
        {
            var shipmentData = _context.SupplyManagementRequest.Include(x => x.PharmacyStudyProductType).Where(x => x.Id == supplyManagementshipmentDto.SupplyManagementRequestId).FirstOrDefault();
            if (shipmentData == null)
            {
                return "Request data not found!";
            }
            var project = _context.Project.Where(x => x.Id == shipmentData.FromProjectId).FirstOrDefault();
            if (project == null)
            {
                return "From project code not found!";
            }
            if (project.Status == Helper.MonitoringSiteStatus.CloseOut || project.Status == Helper.MonitoringSiteStatus.Terminated || project.Status == Helper.MonitoringSiteStatus.OnHold || project.Status == Helper.MonitoringSiteStatus.Rejected)
            {
                return "You can't approve this shipment,Request " + project.ProjectCode + " site is " + project.Status.GetDescription() + "!";
            }
            var settings = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == project.ParentProjectId && x.DeletedDate == null).FirstOrDefault();
            if (settings == null)
            {
                return "Please set kit number setting!";
            }
            var productreciept = _context.ProductReceipt.Where(x => x.ProjectId == project.ParentProjectId
                                && x.Status == Helper.ProductVerificationStatus.Approved).FirstOrDefault();
            if (productreciept == null)
            {
                return "Product receipt is pending!";
            }
            var productVerificationDetail = _context.ProductVerificationDetail.Where(x => x.ProductReceipt.ProjectId == project.ParentProjectId
                                && x.ProductReceipt.Status == Helper.ProductVerificationStatus.Approved).FirstOrDefault();
            if (productVerificationDetail == null)
            {
                return "Product verification is pending!";
            }
            if (supplyManagementshipmentDto.Kits != null && supplyManagementshipmentDto.Kits.Count == 0)
            {
                return "Please select kit!";
            }
            if (supplyManagementshipmentDto.Status == Helper.SupplyMangementShipmentStatus.Approved)
            {
                if (settings != null && settings.IsBlindedStudy == true)
                {
                    if (supplyManagementshipmentDto.Kits != null)
                    {

                        if (supplyManagementshipmentDto.ApprovedQty > _supplyManagementRequestRepository.GetAvailableRemainingQty(supplyManagementshipmentDto.SupplyManagementRequestId, settings))
                        {
                            return "Entered quantity is higher than available quantity!";
                        }
                        if (supplyManagementshipmentDto.Kits.Count > supplyManagementshipmentDto.ApprovedQty)
                        {
                            return "Selected quantity is higher than available quantity!";
                        }
                        if (supplyManagementshipmentDto.Kits.Count == 0)
                        {

                            return "Please select kits!";
                        }
                    }
                }

                else if (shipmentData.PharmacyStudyProductType != null && shipmentData.PharmacyStudyProductType.ProductUnitType == Helper.ProductUnitType.Kit)
                {
                    if (supplyManagementshipmentDto.ApprovedQty > _supplyManagementRequestRepository.GetAvailableRemainingQty(supplyManagementshipmentDto.SupplyManagementRequestId, settings))
                    {
                        return "Entered quantity is higher than available quantity!";
                    }
                    if (supplyManagementshipmentDto.Kits != null && supplyManagementshipmentDto.Kits.Count > supplyManagementshipmentDto.ApprovedQty)
                    {
                        return "Selected quantity is higher than available quantity!";
                    }
                    if (supplyManagementshipmentDto.Kits.Count == 0)
                    {

                        return "Please select kits!";
                    }

                }
                else
                {
                    if (shipmentData.StudyProductTypeId != null)
                    {
                        if (!_supplyManagementRequestRepository.CheckAvailableRemainingQty(supplyManagementshipmentDto.ApprovedQty, (int)project.ParentProjectId, (int)shipmentData.StudyProductTypeId))
                        {
                            return "Approve qty should not greater than remaining qty!";
                        }
                    }
                }
            }
            return "";
        }
        public string GetShipmentNo()
        {
            string no = string.Empty;
            bool isnotexist = false;
            while (!isnotexist)
            {
                var str = GenerateShipmentNo();
                if (!string.IsNullOrEmpty(str))
                {
                    var data = All.Where(x => x.ShipmentNo == str).FirstOrDefault();
                    if (data == null)
                    {
                        isnotexist = true;
                        no = str;
                        break;

                    }
                }
            }
            return no;
        }
        public void Assignkits(SupplyManagementRequest shipmentdata, SupplyManagementShipmentDto supplyManagementshipmentDto)
        {
            var request = _context.SupplyManagementRequest.Where(x => x.Id == supplyManagementshipmentDto.SupplyManagementRequestId).FirstOrDefault();
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == shipmentdata.FromProject.ParentProjectId).FirstOrDefault();
            if (supplyManagementshipmentDto.Status == Helper.SupplyMangementShipmentStatus.Approved)
            {
                if (setting.KitCreationType == KitCreationType.KitWise && supplyManagementshipmentDto.Kits.Count > 0)
                {

                    foreach (var item in supplyManagementshipmentDto.Kits)
                    {
                        var kit = _context.SupplyManagementKITDetail.Where(x => x.Id == item.Id).FirstOrDefault();
                        if (kit != null && supplyManagementshipmentDto.Id > 0)
                        {
                            kit.Status = Helper.KitStatus.Shipped;
                            kit.SupplyManagementShipmentId = supplyManagementshipmentDto.Id;
                            if (request.IsSiteRequest)
                            {
                                kit.ToSiteId = request.FromProjectId;
                            }
                            _context.SupplyManagementKITDetail.Update(kit);

                            SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                            history.SupplyManagementKITDetailId = item.Id;
                            history.SupplyManagementShipmentId = supplyManagementshipmentDto.Id;
                            history.Status = KitStatus.Shipped;
                            history.RoleId = _jwtTokenAccesser.RoleId;
                            _context.SupplyManagementKITDetailHistory.Add(history);
                            _uow.Save();
                        }
                    }
                }
                if (setting.KitCreationType == KitCreationType.SequenceWise && supplyManagementshipmentDto.Kits.Count > 0)
                {
                    foreach (var item in supplyManagementshipmentDto.Kits)
                    {
                        var kit = _context.SupplyManagementKITSeries.Where(x => x.Id == item.Id).FirstOrDefault();
                        if (kit != null && supplyManagementshipmentDto.Id > 0)
                        {

                            kit.Status = Helper.KitStatus.Shipped;
                            kit.SupplyManagementShipmentId = supplyManagementshipmentDto.Id;
                            if (request.IsSiteRequest)
                            {
                                kit.ToSiteId = request.FromProjectId;
                            }
                            _context.SupplyManagementKITSeries.Update(kit);

                            SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                            history.SupplyManagementKITSeriesId = item.Id;
                            history.SupplyManagementShipmentId = kit.SupplyManagementShipmentId;
                            history.Status = KitStatus.Shipped;
                            history.RoleId = _jwtTokenAccesser.RoleId;
                            _context.SupplyManagementKITSeriesDetailHistory.Add(history);
                            _uow.Save();
                        }
                    }
                }
            }
        }
        public void SendShipmentApproveRejecttEmail(int id, SupplyManagementShipment shipment)
        {
            IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();
            SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
            var request = _context.SupplyManagementRequest.Include(x => x.ProjectDesignVisit).Include(x => x.FromProject).Include(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Where(x => x.Id == id).FirstOrDefault();
            if (request != null)
            {
                var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == request.FromProject.ParentProjectId && x.Triggers == SupplyManagementEmailTriggers.ShipmentApproveReject).ToList();
                if (emailconfiglist != null && emailconfiglist.Count > 0)
                {
                    var siteconfig = emailconfiglist.Where(x => x.SiteId > 0).ToList();
                    if (siteconfig.Count > 0)
                    {
                        emailconfig = siteconfig.Where(x => x.SiteId == request.FromProjectId).FirstOrDefault();
                    }
                    else
                    {
                        emailconfig = emailconfiglist.FirstOrDefault();
                    }

                    var allocation = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == request.FromProject.ParentProjectId).FirstOrDefault();
                    var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                    if (details.Count() > 0)
                    {

                        if (request.PharmacyStudyProductType != null && request.PharmacyStudyProductType.ProductType != null)
                            iWRSEmailModel.ProductType = request.PharmacyStudyProductType.ProductType.ProductTypeCode;
                        if (allocation != null && allocation.IsBlindedStudy == true)
                        {
                            iWRSEmailModel.ProductType = "Blinded study";
                        }
                        iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == request.FromProject.ParentProjectId).FirstOrDefault().ProjectCode;
                        iWRSEmailModel.SiteCode = request.FromProject.ProjectCode;
                        var managesite = _context.ManageSite.Where(x => x.Id == request.FromProject.ManageSiteId).FirstOrDefault();
                        if (managesite != null)
                        {
                            iWRSEmailModel.SiteName = managesite.SiteName;
                        }
                        iWRSEmailModel.ActionBy = _jwtTokenAccesser.UserName;
                        iWRSEmailModel.RequestedQty = request.RequestQty;
                        if (request.IsSiteRequest)
                        {
                            iWRSEmailModel.RequestType = "Site to Site Request";
                            if (request.ToProjectId > 0)
                            {
                                var toproject = _context.Project.Where(x => x.Id == request.ToProjectId).FirstOrDefault();
                                if (toproject != null)
                                {
                                    iWRSEmailModel.RequestToSiteCode = toproject.ProjectCode;
                                    var tomanagesite = _context.ManageSite.Where(x => x.Id == toproject.ManageSiteId).FirstOrDefault();
                                    if (tomanagesite != null)
                                    {
                                        iWRSEmailModel.RequestToSiteName = tomanagesite.SiteName;
                                    }

                                }
                                var Projectrights = _context.ProjectRight.Where(x => x.DeletedDate == null && x.ProjectId == request.ToProjectId).ToList();
                                if (Projectrights.Count > 0)
                                    details = details.Where(x => Projectrights.Select(z => z.UserId).Contains(x.UserId)).ToList();

                            }
                        }
                        else
                        {
                            iWRSEmailModel.RequestType = "Site to Study Request";
                        }
                        iWRSEmailModel.Visit = request.ProjectDesignVisit.DisplayName;
                        iWRSEmailModel.Status = shipment.Status == SupplyMangementShipmentStatus.Approved ? "Approved" : "Rejected";
                        iWRSEmailModel.ApprovedQty = shipment.ApprovedQty;
                        iWRSEmailModel.RequestedBy = _context.Users.Where(x => x.Id == request.CreatedBy).FirstOrDefault().UserName;

                        _emailSenderRespository.SendforApprovalEmailIWRS(iWRSEmailModel, details.Select(x => x.Users.Email).Distinct().ToList(), emailconfig);
                        foreach (var item in details)
                        {
                            SupplyManagementEmailConfigurationDetailHistory history = new SupplyManagementEmailConfigurationDetailHistory();
                            history.SupplyManagementEmailConfigurationDetailId = item.Id;
                            _context.SupplyManagementEmailConfigurationDetailHistory.Add(history);
                            _context.Save();
                        }
                    }
                }
            }

        }

        public async Task ShipmentShipmentEmailSchedule()
        {
            int? projectId = 0;
            string recurenceType = string.Empty;
            int? recordId = 0;
            try
            {
                var requestIds = await _context.SupplyManagementReceipt.Where(x => x.SupplyManagementShipmentId > 0).Select(s => s.SupplyManagementShipmentId).ToListAsync();
                var requestdata = await _context.SupplyManagementShipment.Include(s => s.SupplyManagementRequest).ThenInclude(x => x.ProjectDesignVisit).Include(s => s.SupplyManagementRequest).ThenInclude(x => x.FromProject).Include(s => s.SupplyManagementRequest).ThenInclude(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType)
                    .Where(x => x.DeletedDate == null && !requestIds.Contains(x.Id)).ToListAsync();
                if (requestdata != null && requestdata.Count > 0)
                {
                    foreach (var request in requestdata)
                    {
                        SupplyManagementEmailScheduleLog supplyManagementEmailScheduleLog = new SupplyManagementEmailScheduleLog();
                        SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
                        IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();
                        SupplyManagementShipment supplyManagementRequest = new SupplyManagementShipment();
                        var emailconfiglist = await _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == request.SupplyManagementRequest.FromProject.ParentProjectId && x.Triggers == SupplyManagementEmailTriggers.ShipmentApproveReject).ToListAsync();
                        if (emailconfiglist != null && emailconfiglist.Count > 0)
                        {
                           
                            var siteconfig = emailconfiglist.Where(x => x.SiteId > 0).ToList();
                            if (siteconfig.Count > 0)
                            {
                                emailconfig = siteconfig.Where(x => x.SiteId == request.SupplyManagementRequest.FromProjectId).FirstOrDefault();
                            }
                            else
                            {
                                emailconfig = emailconfiglist.FirstOrDefault();
                            }

                            supplyManagementEmailScheduleLog.ProjectId = request.SupplyManagementRequest.FromProject.ParentProjectId;
                            supplyManagementEmailScheduleLog.TriggerType = emailconfig.Triggers.GetDescription();
                            supplyManagementEmailScheduleLog.RecurrenceType = emailconfig.RecurrenceType.GetDescription();
                            supplyManagementEmailScheduleLog.Message = "Shipement Request Schedule Start " + DateTime.Now;
                            supplyManagementEmailScheduleLog.RecordId = request.Id;
                            _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog);

                            projectId = request.SupplyManagementRequest.FromProject.ParentProjectId;
                            recurenceType = emailconfig.RecurrenceType.GetDescription();
                            recordId = request.Id;

                            if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Daily)
                            {
                                supplyManagementRequest = request;
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.AlternateDay)
                            {
                                DateTime start = Convert.ToDateTime(request.CreatedDate);
                                DateTime end = DateTime.Now;
                                TimeSpan span = end.Date - start.Date;
                                double difference = span.TotalDays;
                                if (difference % 2 == 0)
                                    supplyManagementRequest = request;
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Weekly)
                            {
                                DateTime start = Convert.ToDateTime(request.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddDays(7);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = request;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.FifteenDays)
                            {
                                DateTime start = Convert.ToDateTime(request.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddDays(15);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = request;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Monthly)
                            {
                                DateTime start = Convert.ToDateTime(request.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddMonths(1);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = request;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.EveryTwoMonth)
                            {
                                DateTime start = Convert.ToDateTime(request.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddMonths(2);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = request;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Quarterly)
                            {
                                DateTime start = Convert.ToDateTime(request.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddMonths(3);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = request;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.EverySixMonth)
                            {
                                DateTime start = Convert.ToDateTime(request.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddMonths(6);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = request;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Yearly)
                            {
                                DateTime start = Convert.ToDateTime(request.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddYears(1);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = request;
                                        start = end;
                                    }
                                }
                            }
                            if (supplyManagementRequest != null)
                            {
                                var allocation = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == request.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault();
                                var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                                if (details.Count() > 0)
                                {

                                    if (request.SupplyManagementRequest.PharmacyStudyProductType != null && request.SupplyManagementRequest.PharmacyStudyProductType.ProductType != null)
                                        iWRSEmailModel.ProductType = request.SupplyManagementRequest.PharmacyStudyProductType.ProductType.ProductTypeCode;
                                    if (allocation != null && allocation.IsBlindedStudy == true)
                                    {
                                        iWRSEmailModel.ProductType = "Blinded study";
                                    }
                                    iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == request.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault().ProjectCode;
                                    iWRSEmailModel.SiteCode = request.SupplyManagementRequest.FromProject.ProjectCode;
                                    var managesite = _context.ManageSite.Where(x => x.Id == request.SupplyManagementRequest.FromProject.ManageSiteId).FirstOrDefault();
                                    if (managesite != null)
                                    {
                                        iWRSEmailModel.SiteName = managesite.SiteName;
                                    }
                                    iWRSEmailModel.ActionBy = _jwtTokenAccesser.UserName;
                                    iWRSEmailModel.RequestedQty = request.SupplyManagementRequest.RequestQty;
                                    if (request.SupplyManagementRequest.IsSiteRequest)
                                    {
                                        iWRSEmailModel.RequestType = "Site to Site Request";
                                        if (request.SupplyManagementRequest.ToProjectId > 0)
                                        {
                                            var toproject = _context.Project.Where(x => x.Id == request.SupplyManagementRequest.ToProjectId).FirstOrDefault();
                                            if (toproject != null)
                                            {
                                                iWRSEmailModel.RequestToSiteCode = toproject.ProjectCode;
                                                var tomanagesite = _context.ManageSite.Where(x => x.Id == toproject.ManageSiteId).FirstOrDefault();
                                                if (tomanagesite != null)
                                                {
                                                    iWRSEmailModel.RequestToSiteName = tomanagesite.SiteName;
                                                }

                                            }
                                            var Projectrights = _context.ProjectRight.Where(x => x.DeletedDate == null && x.ProjectId == request.SupplyManagementRequest.ToProjectId).ToList();
                                            if (Projectrights.Count > 0)
                                                details = details.Where(x => Projectrights.Select(z => z.UserId).Contains(x.UserId)).ToList();

                                        }
                                    }
                                    else
                                    {
                                        iWRSEmailModel.RequestType = "Site to Study Request";
                                    }
                                    iWRSEmailModel.Visit = request.SupplyManagementRequest.ProjectDesignVisit.DisplayName;
                                    iWRSEmailModel.Status = request.Status == SupplyMangementShipmentStatus.Approved ? "Approved" : "Rejected";
                                    iWRSEmailModel.ApprovedQty = request.ApprovedQty;
                                    iWRSEmailModel.RequestedBy = _context.Users.Where(x => x.Id == request.CreatedBy).FirstOrDefault().UserName;

                                    _emailSenderRespository.SendforApprovalEmailIWRS(iWRSEmailModel, details.Select(x => x.Users.Email).Distinct().ToList(), emailconfig);
                                    foreach (var item in details)
                                    {
                                        SupplyManagementEmailConfigurationDetailHistory history = new SupplyManagementEmailConfigurationDetailHistory();
                                        history.SupplyManagementEmailConfigurationDetailId = item.Id;
                                        _context.SupplyManagementEmailConfigurationDetailHistory.Add(history);
                                        _context.Save();
                                    }
                                }
                            }

                            var supplyManagementEmailScheduleLog1 = new SupplyManagementEmailScheduleLog();
                            supplyManagementEmailScheduleLog1.ProjectId = request.SupplyManagementRequest.FromProject.ParentProjectId;
                            supplyManagementEmailScheduleLog1.TriggerType = emailconfig.Triggers.GetDescription();
                            supplyManagementEmailScheduleLog1.RecurrenceType = emailconfig.RecurrenceType.GetDescription();
                            supplyManagementEmailScheduleLog1.Message = "Shipement Request Schedule end " + DateTime.Now;
                            supplyManagementEmailScheduleLog1.RecordId = request.Id;
                            _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog1);
                            _context.Save();
                        }
                    }
                    
                }
            }
            catch (Exception ex)
            {
                SupplyManagementEmailScheduleLog supplyManagementEmailScheduleLog = new SupplyManagementEmailScheduleLog();
                supplyManagementEmailScheduleLog.Message = ex.Message.ToString();
                supplyManagementEmailScheduleLog.TriggerType = SupplyManagementEmailTriggers.ShipmentApproveReject.GetDescription();
                supplyManagementEmailScheduleLog.ProjectId = projectId;
                supplyManagementEmailScheduleLog.RecurrenceType = recurenceType;
                supplyManagementEmailScheduleLog.RecordId = recordId;
                _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog);
                _context.Save();
            }
        }

        public string ExpiryDateShipmentValidation(SupplyManagementRequest shipmentdata, SupplyManagementShipmentDto supplyManagementshipmentDto)
        {

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == shipmentdata.FromProject.ParentProjectId).FirstOrDefault();
            if (setting != null)
            {
                if (supplyManagementshipmentDto.Status == Helper.SupplyMangementShipmentStatus.Approved)
                {
                    if (setting.KitCreationType == KitCreationType.KitWise && supplyManagementshipmentDto.Kits.Count > 0)
                    {

                        foreach (var item in supplyManagementshipmentDto.Kits)
                        {
                            var kit = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).Where(x => x.Id == item.Id).FirstOrDefault();
                            if (kit != null)
                            {
                                var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == kit.SupplyManagementKIT.ProductReceiptId).FirstOrDefault();
                                if (productreciept == null)
                                    return "Product receipt not found";
                                var currentdate = DateTime.Now.Date;
                                var expiry = Convert.ToDateTime(productreciept.RetestExpiryDate).Date;
                                var date = currentdate.AddDays(-(int)kit.SupplyManagementKIT.Days);
                                if (currentdate.Date < date.Date)
                                {
                                    return "Product is expired";
                                }
                                if (Convert.ToDateTime(productreciept.RetestExpiryDate).Date < Convert.ToDateTime(supplyManagementshipmentDto.EstimatedCourierDate).Date)
                                {
                                    return "Product is expired before estimated courier date";
                                }
                            }
                        }
                    }
                    if (setting.KitCreationType == KitCreationType.SequenceWise && supplyManagementshipmentDto.Kits.Count > 0)
                    {
                        foreach (var item in supplyManagementshipmentDto.Kits)
                        {
                            var kit = _context.SupplyManagementKITSeries.Where(x => x.Id == item.Id).FirstOrDefault();
                            if (kit != null)
                            {
                                var currentdate = DateTime.Now.Date;
                                if (Convert.ToDateTime(kit.KitExpiryDate).Date < currentdate.Date)
                                {
                                    return "Product is expired";
                                }
                                if (Convert.ToDateTime(kit.KitExpiryDate).Date < Convert.ToDateTime(supplyManagementshipmentDto.EstimatedCourierDate).Date)
                                {
                                    return "Product is expired before estimated courier date";
                                }
                            }
                        }
                    }
                }
            }

            return "";
        }
    }
}

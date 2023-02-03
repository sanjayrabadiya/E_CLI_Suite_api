using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
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

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementRequestRepository : GenericRespository<SupplyManagementRequest>, ISupplyManagementRequestRepository
    {

        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEmailSenderRespository _emailSenderRespository;

        public SupplyManagementRequestRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IEmailSenderRespository emailSenderRespository,
            IMapper mapper)
            : base(context)
        {

            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _emailSenderRespository = emailSenderRespository;
        }
        public List<DropDownDto> GetSiteDropdownforShipmentRequest(int ProjectId, int ParenrProjectId)
        {
            return _context.Project.Where(x => x.ParentProjectId == ParenrProjectId && x.Id != ProjectId && x.DeletedDate == null)
                .Select(c => new DropDownDto { Id = c.Id, Value = c.ProjectCode })
                .ToList();
        }
        public ProductUnitType GetPharmacyStudyProductUnitType(int id)
        {
            return _context.PharmacyStudyProductType.Where(x => x.Id == id && x.DeletedDate == null)
                .Select(c => c.ProductUnitType).FirstOrDefault();

        }
        public List<SupplyManagementRequestGridDto> GetShipmentRequestList(int parentProjectId, int SiteId, bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.FromProjectId == SiteId).
                    ProjectTo<SupplyManagementRequestGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(t =>
            {
                var shipmentdata = _context.SupplyManagementShipment.Include(z => z.CreatedByUser).Where(x =>
                 x.SupplyManagementRequestId == t.Id).FirstOrDefault();
                if (shipmentdata != null)
                {
                    t.ApprovedQty = shipmentdata.ApprovedQty;
                    t.SupplyManagementShipmentId = shipmentdata.Id;
                    t.Status = shipmentdata.Status.GetDescription();
                    t.ApproveRejectDateTime = shipmentdata.CreatedDate;
                    t.AuditReason = shipmentdata.AuditReasonId != null ? _context.AuditReason.Where(x => x.Id == shipmentdata.AuditReasonId).FirstOrDefault().ReasonName : "";
                    t.ReasonOth = shipmentdata.ReasonOth;
                    t.ApproveRejectBy = shipmentdata.CreatedByUser != null ? shipmentdata.CreatedByUser.UserName : "";
                }

                var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                if (fromproject != null)
                {
                    var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                    t.StudyProjectCode = study != null ? study.ProjectCode : "";
                }
                t.siteRequest = t.IsSiteRequest ? "Site to Site" : "Site to Study";

            });

            return data.Where(x => x.Status == null || x.Status == "" || x.Status == "Reject").ToList();
        }
        public bool CheckAvailableRemainingQty(int reqQty, int ProjectId, int PharmacyStudyProductTypeId)
        {
            bool isAvailable = true;
            var RemainingQuantity = _context.ProductVerificationDetail.Where(x => x.ProductReceipt.ProjectId == ProjectId
                 && x.ProductReceipt.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId
                 && x.ProductReceipt.Status == ProductVerificationStatus.Approved)
                 .Sum(z => z.RemainingQuantity);
            if (RemainingQuantity > 0)
            {
                var approvedQty = _context.SupplyManagementShipment.Where(x => x.DeletedDate == null
                 && x.SupplyManagementRequest.FromProject.ParentProjectId == ProjectId && x.SupplyManagementRequest.StudyProductTypeId == PharmacyStudyProductTypeId
                 && x.Status == SupplyMangementShipmentStatus.Approved).Sum(x => x.ApprovedQty);

                var finalRemainingQty = RemainingQuantity - approvedQty;
                if (reqQty > finalRemainingQty)
                {
                    isAvailable = false;
                }
            }
            else
            {
                isAvailable = false;
            }
            return isAvailable;
        }
        public int GetAvailableRemainingQty(int ProjectId, int PharmacyStudyProductTypeId)
        {

            var RemainingQuantity = _context.ProductVerificationDetail.Where(x => x.ProductReceipt.ProjectId == ProjectId
                 && x.ProductReceipt.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId
                 && x.ProductReceipt.Status == ProductVerificationStatus.Approved)
                 .Sum(z => z.RemainingQuantity);
            if (RemainingQuantity > 0)
            {
                var approvedQty = _context.SupplyManagementShipment.Where(x => x.DeletedDate == null
                 && x.SupplyManagementRequest.FromProject.ParentProjectId == ProjectId && x.SupplyManagementRequest.StudyProductTypeId == PharmacyStudyProductTypeId
                 && x.Status == SupplyMangementShipmentStatus.Approved).Sum(x => x.ApprovedQty);

                var finalRemainingQty = RemainingQuantity - approvedQty;
                return finalRemainingQty;

            }

            return 0;
        }
        public int GetAvailableRemainingKit(int SupplyManagementRequestId)
        {
            int remainingKit;
            var obj = All.Include(x => x.FromProject).Where(x => x.Id == SupplyManagementRequestId).FirstOrDefault();
            if (obj == null)
                return 0;
            if (obj.IsSiteRequest)
            {
                remainingKit = _context.SupplyManagementKITDetail.Where(x =>
                        x.SupplyManagementKIT.PharmacyStudyProductTypeId == obj.StudyProductTypeId &&
                        x.SupplyManagementShipmentId == null
                        && x.SupplyManagementKIT.SiteId != null
                        && x.SupplyManagementKIT.SiteId == obj.ToProjectId
                        && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.Returned)
                        && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                        && x.DeletedDate == null).Count();
            }
            else
            {
                remainingKit = _context.SupplyManagementKITDetail.Where(x =>
                                 x.SupplyManagementKIT.PharmacyStudyProductTypeId == obj.StudyProductTypeId
                                 && x.SupplyManagementKIT.SiteId == null
                                 && (x.Status == KitStatus.AllocationPending || (x.Status == KitStatus.Returned && x.IsUnUsed == true))
                                 && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                 && x.SupplyManagementKIT.ProjectId == obj.FromProject.ParentProjectId
                                 && x.DeletedDate == null).Count();
            }
            return remainingKit;
        }
        public int GetAvailableRemainingKitBlindedStudy(int SupplyManagementRequestId)
        {
            int remainingKit;
            var obj = All.Include(x => x.FromProject).Where(x => x.Id == SupplyManagementRequestId).FirstOrDefault();
            if (obj == null)
                return 0;
            if (obj.IsSiteRequest)
            {
                remainingKit = _context.SupplyManagementKITDetail.Where(x =>
                        x.SupplyManagementShipmentId == null
                        && x.SupplyManagementKIT.SiteId != null
                        && x.SupplyManagementKIT.SiteId == obj.ToProjectId
                        && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.Returned)
                        && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                        && x.DeletedDate == null).Count();
            }
            else
            {
                remainingKit = _context.SupplyManagementKITDetail.Where(x =>
                                  x.SupplyManagementKIT.SiteId == null
                                 && (x.Status == KitStatus.AllocationPending || (x.Status == KitStatus.Returned && x.IsUnUsed == true))
                                 && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                 && x.SupplyManagementKIT.ProjectId == obj.FromProject.ParentProjectId
                                 && x.DeletedDate == null).Count();
            }
            return remainingKit;
        }
        public List<KitListApprove> GetAvailableKit(int SupplyManagementRequestId)
        {
            var obj = All.Include(x => x.FromProject).Where(x => x.Id == SupplyManagementRequestId).FirstOrDefault();
            if (obj == null)
                return new List<KitListApprove>();
            var data = new List<KitListApprove>();
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == obj.FromProject.ParentProjectId).FirstOrDefault();
            if (setting != null && setting.IsBlindedStudy == true)
            {
                if (obj.IsSiteRequest)
                {
                    data = _context.SupplyManagementKITDetail.Where(x =>

                            x.SupplyManagementShipmentId == null
                            && x.SupplyManagementKIT.SiteId != null
                            && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.Returned)
                            && x.SupplyManagementKIT.SiteId == obj.ToProjectId
                            && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                            && x.DeletedDate == null).Select(x => new KitListApprove
                            {
                                Id = x.Id,
                                KitNo = x.KitNo,
                                VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                SiteCode = x.SupplyManagementKIT.Site.ProjectCode,
                                ProductCode = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode
                            }).OrderBy(x => x.KitNo).ToList();
                }
                else
                {
                    data = _context.SupplyManagementKITDetail.Where(x =>
                                 x.SupplyManagementKIT.SiteId == null
                                 && (x.Status == KitStatus.AllocationPending || (x.Status == KitStatus.Returned && x.IsUnUsed == true))
                                 && x.SupplyManagementKIT.ProjectId == obj.FromProject.ParentProjectId
                                 && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                 && x.DeletedDate == null).Select(x => new KitListApprove
                                 {
                                     Id = x.Id,
                                     KitNo = x.KitNo,
                                     VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                     SiteCode = x.SupplyManagementKIT.Site.ProjectCode,
                                     ProductCode = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode
                                 }).OrderBy(x => x.KitNo).ToList();
                }
            }
            else
            {

                if (obj.IsSiteRequest)
                {
                    data = _context.SupplyManagementKITDetail.Where(x =>
                            x.SupplyManagementKIT.PharmacyStudyProductTypeId == obj.StudyProductTypeId &&
                            x.SupplyManagementShipmentId == null
                            && x.SupplyManagementKIT.SiteId != null
                            && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.Returned)
                            && x.SupplyManagementKIT.SiteId == obj.ToProjectId
                            && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                            && x.DeletedDate == null).Select(x => new KitListApprove
                            {
                                Id = x.Id,
                                KitNo = x.KitNo,
                                VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                SiteCode = x.SupplyManagementKIT.Site.ProjectCode,
                                ProductCode = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode
                            }).OrderBy(x => x.KitNo).Take(obj.RequestQty).ToList();


                }
                else
                {

                    data = _context.SupplyManagementKITDetail.Where(x =>
                                     x.SupplyManagementKIT.PharmacyStudyProductTypeId == obj.StudyProductTypeId
                                     && x.SupplyManagementKIT.SiteId == null
                                     && (x.Status == KitStatus.AllocationPending || (x.Status == KitStatus.Returned && x.IsUnUsed == true))
                                     && x.SupplyManagementKIT.ProjectId == obj.FromProject.ParentProjectId
                                     && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                     && x.DeletedDate == null).Select(x => new KitListApprove
                                     {
                                         Id = x.Id,
                                         KitNo = x.KitNo,
                                         VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                         SiteCode = x.SupplyManagementKIT.Site.ProjectCode,
                                         ProductCode = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode
                                     }).OrderBy(x => x.KitNo).Take(obj.RequestQty).ToList();


                }
            }
            return data;
        }
        public void SendrequestEmail(int id)
        {
            SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
            IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();
            var request = _context.SupplyManagementRequest.Include(x => x.ProjectDesignVisit).Include(x => x.FromProject).Include(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Where(x => x.Id == id).FirstOrDefault();
            if (request != null)
            {
                var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == request.FromProject.ParentProjectId && x.Triggers == SupplyManagementEmailTriggers.ShipmentRequest).ToList();
                if (emailconfig != null)
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
                        iWRSEmailModel.RequestedBy = _jwtTokenAccesser.UserName;
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
    }
}

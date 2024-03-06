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
using System.Threading.Tasks;


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
            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                             Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == parentProjectId
                             && s.RoleId == _jwtTokenAccesser.RoleId);
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == parentProjectId).FirstOrDefault();

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
                    var audit = _context.AuditReason.Where(x => x.Id == shipmentdata.AuditReasonId).FirstOrDefault();
                    if (audit != null)
                        t.AuditReason = audit.ReasonName;
                    t.ReasonOth = shipmentdata.ReasonOth;
                    t.ApproveRejectBy = shipmentdata.CreatedByUser != null ? shipmentdata.CreatedByUser.UserName : "";
                }

                var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                if (fromproject != null)
                {
                    var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                    t.StudyProjectCode = study != null ? study.ProjectCode : "";
                    if (setting != null)
                        t.StudyProductTypeName = setting.IsBlindedStudy == true && isShow ? "" : t.StudyProductTypeName;
                }
                t.siteRequest = t.IsSiteRequest ? "Site to Site" : "Site to Study";

            });

            return data.Where(x => x.Status == null || x.Status == "" || x.Status == "Rejected").ToList();
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
        public int GetAvailableRemainingQty(int SupplyManagementRequestId, SupplyManagementKitNumberSettings supplyManagementKitNumberSettings)
        {
            int remainingKit = 0;
            var obj = All.Include(x => x.FromProject).Include(x => x.PharmacyStudyProductType).Where(x => x.Id == SupplyManagementRequestId).FirstOrDefault();
            if (obj == null)
                return 0;
            if (supplyManagementKitNumberSettings == null)
            {
                remainingKit = _context.ProductVerificationDetail.Where(x => x.ProductReceipt.ProjectId == obj.FromProject.ParentProjectId
                    && x.ProductReceipt.PharmacyStudyProductTypeId == obj.StudyProductTypeId
                    && x.ProductReceipt.Status == ProductVerificationStatus.Approved)
                    .Sum(z => z.RemainingQuantity);
                if (remainingKit > 0)
                {
                    var approvedQty = _context.SupplyManagementShipment.Where(x => x.DeletedDate == null
                     && x.SupplyManagementRequest.FromProject.ParentProjectId == obj.FromProject.ParentProjectId && x.SupplyManagementRequest.StudyProductTypeId == obj.StudyProductTypeId
                     && x.Status == SupplyMangementShipmentStatus.Approved).Sum(x => x.ApprovedQty);

                    var finalRemainingQty = remainingKit - approvedQty;
                    return finalRemainingQty;

                }
            }

            if (supplyManagementKitNumberSettings != null && supplyManagementKitNumberSettings.IsBlindedStudy == true)
            {
                if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                {
                    if (obj.IsSiteRequest)
                    {
                        remainingKit = _context.SupplyManagementKITDetail.Where(x =>
                                x.SupplyManagementKIT.SiteId == obj.ToProjectId
                                && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                && x.DeletedDate == null).Count();

                        remainingKit = remainingKit + _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                  x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ToProjectId
                                  && (x.Status == KitStatus.WithoutIssue || x.Status == KitStatus.WithIssue)
                                  && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                  && x.DeletedDate == null).Count();
                    }
                    else
                    {
                        remainingKit = _context.SupplyManagementKITDetail.Where(x =>
                                          x.SupplyManagementKIT.SiteId == null
                                         && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                         && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                         && x.SupplyManagementKIT.ProjectId == obj.FromProject.ParentProjectId
                                         && x.DeletedDate == null).Count();
                    }
                }
                if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.SequenceWise)
                {
                    if (obj.IsSiteRequest)
                    {
                        remainingKit = _context.SupplyManagementKITSeries.Where(x =>
                                x.SiteId == obj.ToProjectId
                                && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                && x.DeletedDate == null).Count();

                        remainingKit = remainingKit + _context.SupplyManagementKITSeries.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                   x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ToProjectId
                                  && (x.Status == KitStatus.WithoutIssue || x.Status == KitStatus.WithIssue)
                                  && x.DeletedDate == null).Count();


                    }
                    else
                    {
                        remainingKit = _context.SupplyManagementKITSeries.Where(x =>
                                          x.SiteId == null
                                         && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                         && x.ProjectId == obj.FromProject.ParentProjectId
                                         && x.DeletedDate == null).Count();
                    }
                }
            }
            else
            {
                if (supplyManagementKitNumberSettings != null && supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                {
                    if (obj.PharmacyStudyProductType != null && obj.PharmacyStudyProductType.ProductUnitType == Helper.ProductUnitType.Kit)
                    {
                        if (obj.IsSiteRequest)
                        {
                            remainingKit = _context.SupplyManagementKITDetail.Where(x =>
                                    x.SupplyManagementKIT.PharmacyStudyProductTypeId == obj.StudyProductTypeId
                                    && x.SupplyManagementKIT.SiteId != null
                                    && x.SupplyManagementKIT.SiteId == obj.ToProjectId
                                    && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                    && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                    && x.DeletedDate == null).Count();

                            remainingKit = remainingKit + _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                        x.SupplyManagementKIT.PharmacyStudyProductTypeId == obj.StudyProductTypeId
                                        && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ToProjectId
                                        && (x.Status == KitStatus.WithoutIssue || x.Status == KitStatus.WithIssue)
                                        && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                        && x.DeletedDate == null).Count();
                        }
                        else
                        {
                            remainingKit = _context.SupplyManagementKITDetail.Where(x =>
                                             x.SupplyManagementKIT.PharmacyStudyProductTypeId == obj.StudyProductTypeId &&
                                             x.SupplyManagementKIT.SiteId == null
                                             && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                             && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                             && x.SupplyManagementKIT.ProjectId == obj.FromProject.ParentProjectId
                                             && x.DeletedDate == null).Count();
                        }
                    }
                    else
                    {
                        if (obj.IsSiteRequest)
                        {
                            remainingKit = _context.SupplyManagementKITDetail.Where(x =>
                                    x.SupplyManagementKIT.SiteId != null
                                    && x.SupplyManagementKIT.SiteId == obj.ToProjectId
                                    && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                    && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                    && x.DeletedDate == null).Count();

                            remainingKit = remainingKit + _context.SupplyManagementKITDetail.Include(s => s.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                         x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ToProjectId
                                         && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                         && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                         && x.DeletedDate == null).Count();
                        }
                        else
                        {
                            remainingKit = _context.SupplyManagementKITDetail.Where(x =>
                                              x.SupplyManagementKIT.SiteId == null
                                             && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                             && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                             && x.SupplyManagementKIT.ProjectId == obj.FromProject.ParentProjectId
                                             && x.DeletedDate == null).Count();
                        }
                    }
                }
                if (supplyManagementKitNumberSettings != null && supplyManagementKitNumberSettings.KitCreationType == KitCreationType.SequenceWise)
                {
                    if (obj.IsSiteRequest)
                    {
                        remainingKit = _context.SupplyManagementKITSeries.Where(x =>
                                x.SiteId == obj.ToProjectId
                                && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                && x.DeletedDate == null).Count();

                        remainingKit = remainingKit + _context.SupplyManagementKITSeries.Include(s => s.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                   x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ToProjectId
                                   && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                   && x.DeletedDate == null).Count();
                    }
                    else
                    {
                        remainingKit = _context.SupplyManagementKITSeries.Where(x =>
                                          x.SiteId == null
                                         && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                         && x.ProjectId == obj.FromProject.ParentProjectId
                                         && x.DeletedDate == null).Count();
                    }
                }

            }


            return remainingKit;
        }
        public List<KitListApprove> GetAvailableKit(int SupplyManagementRequestId)
        {
            var obj = All.Include(x => x.FromProject).Where(x => x.Id == SupplyManagementRequestId).FirstOrDefault();
            if (obj == null)
                return new List<KitListApprove>();

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == obj.FromProject.ParentProjectId).FirstOrDefault();
            if (setting == null)
                return new List<KitListApprove>();

            if (setting.KitCreationType == KitCreationType.KitWise)
            {
                if (obj.IsSiteRequest)
                {
                    var data = _context.SupplyManagementKITDetail.Where(x =>
                             x.SupplyManagementKIT.PharmacyStudyProductTypeId == (setting.IsBlindedStudy == true ? x.SupplyManagementKIT.PharmacyStudyProductTypeId : obj.StudyProductTypeId)
                             && x.SupplyManagementKIT.SiteId != null
                             && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                             && x.SupplyManagementKIT.SiteId == obj.ToProjectId
                             && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                             && x.DeletedDate == null).Select(x => new KitListApprove
                             {
                                 Id = x.Id,
                                 KitNo = x.KitNo,
                                 VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                 SiteCode = x.SupplyManagementKIT.Site.ProjectCode,
                                 ProductCode = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode,
                                 RetestExpiry = x.SupplyManagementKIT.ProductReceiptId > 0 ? _context.ProductVerification.Where(s => s.ProductReceiptId == x.SupplyManagementKIT.ProductReceiptId).FirstOrDefault().RetestExpiryDate : null,
                                 LotBatchNo = x.SupplyManagementKIT.ProductReceiptId > 0 ? _context.ProductVerification.Where(s => s.ProductReceiptId == x.SupplyManagementKIT.ProductReceiptId).FirstOrDefault().BatchLotNumber : "",
                                 Dose = x.SupplyManagementKIT.Dose,
                                 Barcode = x.Barcode,
                                 Isdisable = setting.IsBarcodeScan
                             }).OrderBy(x => x.KitNo).ToList();


                    var data1 = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(s => s.SupplyManagementRequest).ThenInclude(s => s.FromProject).Where(x =>
                                  x.SupplyManagementKIT.PharmacyStudyProductTypeId == (setting.IsBlindedStudy == true ? x.SupplyManagementKIT.PharmacyStudyProductTypeId : obj.StudyProductTypeId)

                                  && (x.Status == KitStatus.WithoutIssue || x.Status == KitStatus.WithIssue)
                                  && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ToProjectId
                                  && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                  && x.DeletedDate == null).Select(x => new KitListApprove
                                  {
                                      Id = x.Id,
                                      KitNo = x.KitNo,
                                      VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                      SiteCode = x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode,
                                      ProductCode = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode,
                                      RetestExpiry = x.SupplyManagementKIT.ProductReceiptId > 0 ? _context.ProductVerification.Where(s => s.ProductReceiptId == x.SupplyManagementKIT.ProductReceiptId).FirstOrDefault().RetestExpiryDate : null,
                                      LotBatchNo = x.SupplyManagementKIT.ProductReceiptId > 0 ? _context.ProductVerification.Where(s => s.ProductReceiptId == x.SupplyManagementKIT.ProductReceiptId).FirstOrDefault().BatchLotNumber : "",
                                      Dose = x.SupplyManagementKIT.Dose,
                                      Barcode = x.Barcode,
                                      Isdisable = setting.IsBarcodeScan
                                  }).OrderBy(x => x.KitNo).ToList();

                    data.AddRange(data1);
                    return data;
                }
                else
                {
                    var data = _context.SupplyManagementKITDetail.Where(x =>
                                  x.SupplyManagementKIT.SiteId == null
                                  && x.SupplyManagementKIT.PharmacyStudyProductTypeId == (setting.IsBlindedStudy == true ? x.SupplyManagementKIT.PharmacyStudyProductTypeId : obj.StudyProductTypeId)
                                  && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                                  && x.SupplyManagementKIT.ProjectId == obj.FromProject.ParentProjectId
                                  && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                  && x.DeletedDate == null).Select(x => new KitListApprove
                                  {
                                      Id = x.Id,
                                      KitNo = x.KitNo,
                                      VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                      SiteCode = x.SupplyManagementKIT.Site.ProjectCode,
                                      ProductCode = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode,
                                      RetestExpiry = x.SupplyManagementKIT.ProductReceiptId > 0 ? _context.ProductVerification.Where(s => s.ProductReceiptId == x.SupplyManagementKIT.ProductReceiptId).FirstOrDefault().RetestExpiryDate : null,
                                      LotBatchNo = x.SupplyManagementKIT.ProductReceiptId > 0 ? _context.ProductVerification.Where(s => s.ProductReceiptId == x.SupplyManagementKIT.ProductReceiptId).FirstOrDefault().BatchLotNumber : "",
                                      Dose = x.SupplyManagementKIT.Dose,
                                      Barcode = x.Barcode,
                                      Isdisable = setting.IsBarcodeScan
                                  }).OrderBy(x => x.KitNo).ToList();
                    return data;
                }
            }
            else
            {

                if (obj.IsSiteRequest)
                {
                    var data = _context.SupplyManagementKITSeries.Where(x =>
                             x.SiteId != null
                             && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                             && x.SiteId == obj.ToProjectId
                             && x.DeletedDate == null).Select(x => new KitListApprove
                             {
                                 Id = x.Id,
                                 KitNo = x.KitNo,
                                 ProjectCode = x.Project.ProjectCode,
                                 TreatmentType = x.TreatmentType,
                                 SiteCode = x.SiteId > 0 ? _context.Project.Where(s => s.Id == x.SiteId).FirstOrDefault().ProjectCode : "",
                                 KitValidity = x.KitExpiryDate,
                                 Barcode = x.Barcode,
                                 Isdisable = setting.IsBarcodeScan
                             }).OrderBy(x => x.KitNo).ToList();

                    var data1 = _context.SupplyManagementKITSeries.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x =>
                                   (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                   && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ToProjectId
                                   && x.DeletedDate == null).Select(x => new KitListApprove
                                   {
                                       Id = x.Id,
                                       KitNo = x.KitNo,
                                       ProjectCode = x.Project.ProjectCode,
                                       TreatmentType = x.TreatmentType,
                                       SiteCode = x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode,
                                       KitValidity = x.KitExpiryDate,
                                       Barcode = x.Barcode,
                                       Isdisable = setting.IsBarcodeScan
                                   }).OrderBy(x => x.KitNo).ToList();

                    data.AddRange(data1);
                    return data;

                }
                else
                {

                    var data = _context.SupplyManagementKITSeries.Where(x =>
                             x.SiteId == null
                             && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.ReturnReceiveWithIssue || x.Status == KitStatus.ReturnReceiveWithoutIssue)
                             && x.ProjectId == obj.FromProject.ParentProjectId
                             && x.DeletedDate == null).Select(x => new KitListApprove
                             {
                                 Id = x.Id,
                                 KitNo = x.KitNo,
                                 ProjectCode = x.Project.ProjectCode,
                                 TreatmentType = x.TreatmentType,
                                 KitValidity = x.KitExpiryDate,
                                 Barcode = x.Barcode,
                                 Isdisable = setting.IsBarcodeScan
                             }).OrderBy(x => x.KitNo).ToList();
                    return data;

                }
            }
        }
        public void SendrequestApprovalEmail(int id)
        {
            IwrsEmailModel iWRSEmailModel = new IwrsEmailModel();
            var request = _context.SupplyManagementRequest.Include(x => x.ProjectDesignVisit).Include(x => x.FromProject).Include(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Where(x => x.Id == id).FirstOrDefault();
            if (request != null)
            {
                var emailconfiglist = _context.SupplyManagementApprovalDetails.Include(s => s.Users).Include(s => s.SupplyManagementApproval).ThenInclude(s => s.Project).Where(x => x.DeletedDate == null && x.SupplyManagementApproval.ProjectId == request.FromProject.ParentProjectId
                && x.SupplyManagementApproval.ApprovalType == Helper.SupplyManagementApprovalType.ShipmentApproval).ToList();
                if (emailconfiglist.Any())
                {
                    var allocation = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == request.FromProject.ParentProjectId).FirstOrDefault();


                    if (request.PharmacyStudyProductType != null && request.PharmacyStudyProductType.ProductType != null)
                        iWRSEmailModel.ProductType = request.PharmacyStudyProductType.ProductType.ProductTypeCode;
                    if (allocation != null && allocation.IsBlindedStudy == true)
                    {
                        iWRSEmailModel.ProductType = "Blinded study";
                    }
                    var project = _context.Project.Where(x => x.Id == request.FromProject.ParentProjectId).FirstOrDefault();
                    if (project != null)
                        iWRSEmailModel.StudyCode = project.ProjectCode;
                    iWRSEmailModel.RequestFromSiteCode = request.FromProject.ProjectCode;
                    var managesite = _context.ManageSite.Where(x => x.Id == request.FromProject.ManageSiteId).FirstOrDefault();
                    if (managesite != null)
                    {
                        iWRSEmailModel.RequestFromSiteName = managesite.SiteName;
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
                                emailconfiglist = emailconfiglist.Where(x => Projectrights.Select(z => z.UserId).Contains(x.UserId)).ToList();

                        }
                    }
                    else
                    {
                        iWRSEmailModel.RequestType = "Site to Study Request";
                    }
                    if (request.ProjectDesignVisit != null)
                        iWRSEmailModel.Visit = request.ProjectDesignVisit.DisplayName;
                    var obj = emailconfiglist.FirstOrDefault();
                    if (obj != null)
                        _emailSenderRespository.SendforShipmentApprovalEmailIWRS(iWRSEmailModel, emailconfiglist.Select(x => x.Users.Email).Distinct().ToList(), obj.SupplyManagementApproval);


                }
            }

        }

        public void SendrequestEmail(int id)
        {
            SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
            IwrsEmailModel iWRSEmailModel = new IwrsEmailModel();
            var request = _context.SupplyManagementRequest.Include(x => x.ProjectDesignVisit).Include(x => x.FromProject).Include(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Where(x => x.Id == id).FirstOrDefault();
            if (request != null)
            {
                var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive && x.ProjectId == request.FromProject.ParentProjectId && x.Triggers == SupplyManagementEmailTriggers.ShipmentRequest).ToList();
                if (emailconfiglist.Any())
                {

                    var siteconfig = emailconfiglist.Where(x => x.SiteId > 0).ToList();
                    if (siteconfig.Count > 0)
                    {
                        emailconfig = siteconfig.Find(x => x.SiteId == request.FromProjectId);
                    }
                    else
                    {
                        emailconfig = emailconfiglist.FirstOrDefault();
                    }

                    var allocation = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == request.FromProject.ParentProjectId).FirstOrDefault();
                    var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                    if (details.Any())
                    {

                        if (request.PharmacyStudyProductType != null && request.PharmacyStudyProductType.ProductType != null)
                            iWRSEmailModel.ProductType = request.PharmacyStudyProductType.ProductType.ProductTypeCode;
                        if (allocation != null && allocation.IsBlindedStudy == true)
                        {
                            iWRSEmailModel.ProductType = "Blinded study";
                        }
                        var project = _context.Project.Where(x => x.Id == request.FromProject.ParentProjectId).FirstOrDefault();
                        if (project != null)
                            iWRSEmailModel.StudyCode = project.ProjectCode;
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
                        if (request.ProjectDesignVisit != null)
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
        public async Task ShipmentRequestEmailSchedule()
        {
            int? projectId = 0;
            int? recordId = 0;
            string recurenceType = string.Empty;
            try
            {
                var requestIds = await _context.SupplyManagementShipment.Where(x => x.SupplyManagementRequestId > 0).Select(s => s.SupplyManagementRequestId).ToListAsync();
                var requestdata = await _context.SupplyManagementRequest.Include(x => x.ProjectDesignVisit).Include(x => x.FromProject).Include(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Where(x => x.DeletedDate == null && !requestIds.Contains(x.Id)).ToListAsync();
                if (requestdata != null && requestdata.Count > 0)
                {
                    foreach (var request in requestdata)
                    {
                        SupplyManagementEmailScheduleLog supplyManagementEmailScheduleLog = new SupplyManagementEmailScheduleLog();
                        SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
                        IwrsEmailModel iWRSEmailModel = new IwrsEmailModel();
                        SupplyManagementRequest supplyManagementRequest = new SupplyManagementRequest();
                        var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive && x.ProjectId == request.FromProject.ParentProjectId && x.Triggers == SupplyManagementEmailTriggers.ShipmentRequest).ToList();
                        if (emailconfiglist != null && emailconfiglist.Count > 0)
                        {

                            var siteconfig = emailconfiglist.Where(x => x.SiteId > 0).ToList();
                            if (siteconfig.Count > 0)
                            {
                                emailconfig = siteconfig.Find(x => x.SiteId == request.FromProjectId);
                            }
                            else
                            {
                                emailconfig = emailconfiglist.FirstOrDefault();
                            }
                            supplyManagementEmailScheduleLog.ProjectId = request.FromProject.ParentProjectId;
                            supplyManagementEmailScheduleLog.TriggerType = emailconfig.Triggers.GetDescription();
                            supplyManagementEmailScheduleLog.RecurrenceType = emailconfig.RecurrenceType.GetDescription();
                            supplyManagementEmailScheduleLog.Message = "Shipement Request Schedule Start " + DateTime.Now;
                            supplyManagementEmailScheduleLog.RecordId = request.Id;
                            _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog);
                            _context.Save();

                            projectId = request.FromProject.ParentProjectId;
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
                                var allocation = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == request.FromProject.ParentProjectId).FirstOrDefault();
                                var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                                if (details.Any())
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
                                    if (request.ProjectDesignVisit != null)
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

                            var supplyManagementEmailScheduleLog1 = new SupplyManagementEmailScheduleLog();
                            supplyManagementEmailScheduleLog1.ProjectId = request.FromProject.ParentProjectId;
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
                supplyManagementEmailScheduleLog.TriggerType = SupplyManagementEmailTriggers.ShipmentRequest.GetDescription();
                supplyManagementEmailScheduleLog.ProjectId = projectId;
                supplyManagementEmailScheduleLog.RecurrenceType = recurenceType;
                supplyManagementEmailScheduleLog.RecordId = recordId;
                _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog);
                _context.Save();
            }
        }

        public string CheckValidationShipmentRequest(SupplyManagementRequestDto supplyManagementRequestDto)
        {
            var project = _context.Project.Where(x => x.Id == supplyManagementRequestDto.FromProjectId).FirstOrDefault();
            if (project == null)
            {
                return " site not found";
            }
            if (project.Status == Helper.MonitoringSiteStatus.CloseOut || project.Status == Helper.MonitoringSiteStatus.Terminated || project.Status == Helper.MonitoringSiteStatus.OnHold || project.Status == Helper.MonitoringSiteStatus.Rejected)
            {
                return " site " + project.ProjectCode + " is " + project.Status.GetDescription() + "!";
            }
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == project.ParentProjectId).FirstOrDefault();
            if (setting == null)
            {
                return "Please set kit number setting!";
            }

            return "";
        }
    }
}

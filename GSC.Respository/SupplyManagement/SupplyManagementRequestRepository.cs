using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
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
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementRequestRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
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
        public List<SupplyManagementRequestGridDto> GetShipmentRequestList(bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
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
                                 && x.SupplyManagementShipmentId == null
                                 && x.SupplyManagementKIT.SiteId == null
                                 && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.Returned)
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
                            SiteCode = x.SupplyManagementKIT.Site.ProjectCode
                        }).OrderBy(x => x.KitNo).Take(obj.RequestQty).ToList();
            }
            else
            {
                data = _context.SupplyManagementKITDetail.Where(x =>
                                 x.SupplyManagementKIT.PharmacyStudyProductTypeId == obj.StudyProductTypeId
                                 && x.SupplyManagementShipmentId == null
                                 && x.SupplyManagementKIT.SiteId == null
                                 && (x.Status == KitStatus.AllocationPending || x.Status == KitStatus.Returned)
                                 && x.SupplyManagementKIT.ProjectId == obj.FromProject.ParentProjectId
                                 && x.SupplyManagementKIT.ProjectDesignVisitId == obj.VisitId
                                 && x.DeletedDate == null).Select(x => new KitListApprove
                                 {
                                     Id = x.Id,
                                     KitNo = x.KitNo,
                                     VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                     SiteCode = x.SupplyManagementKIT.Site.ProjectCode
                                 }).OrderBy(x => x.KitNo).Take(obj.RequestQty).ToList();
            }

            return data;
        }
    }
}

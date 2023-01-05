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
    public class SupplyManagementReceiptRepository : GenericRespository<SupplyManagementReceipt>, ISupplyManagementReceiptRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementReceiptRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }
        public List<SupplyManagementReceiptGridDto> GetSupplyShipmentReceiptList(int parentProjectId, int SiteId, bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == SiteId).
                    ProjectTo<SupplyManagementReceiptGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            data.ForEach(t =>
            {
                var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                if (fromproject != null)
                {
                    var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                    t.StudyProjectCode = study != null ? study.ProjectCode : "";

                }
                t.WithIssueName = t.WithIssue == true ? "Yes" : "No";
            });


            var requestdata = _context.SupplyManagementShipment.Where(x =>
                !data.Select(x => x.SupplyManagementShipmentId).Contains(x.Id)
                && x.SupplyManagementRequest.FromProjectId == SiteId
                && x.Status == SupplyMangementShipmentStatus.Approved && x.DeletedDate == null).
                 ProjectTo<SupplyManagementReceiptGridDto>(_mapper.ConfigurationProvider).ToList();
            requestdata.ForEach(t =>
            {
                SupplyManagementReceiptGridDto obj = new SupplyManagementReceiptGridDto();
                obj = t;
                obj.Id = 0;
                obj.ApproveRejectDateTime = t.CreatedDate;
                obj.WithIssueName = "";
                obj.CreatedByUser = null;
                obj.CreatedDate = null;
                obj.WithIssue = null;
                //obj.SupplyManagementShipmentId = t.Id;
                var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                if (fromproject != null)
                {
                    var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                    obj.StudyProjectCode = study != null ? study.ProjectCode : "";
                }
                data.Add(obj);
            });
            return data.OrderByDescending(x => x.ApproveRejectDateTime).ToList();
        }

        public List<SupplyManagementReceiptHistoryGridDto> GetSupplyShipmentReceiptHistory(int id)
        {
            int requestid = 0;
            int shipmentid = 0;
            int recieptid = 0;
            var data = All.Include(x => x.SupplyManagementShipment).Where(x => x.Id == id).FirstOrDefault();
            if (data == null)
            {
                var data1 = _context.SupplyManagementShipment.Include(x => x.SupplyManagementRequest).Where(x => x.Id == id).FirstOrDefault();
                requestid = data1.SupplyManagementRequestId;
                shipmentid = data1.Id;

            }
            else
            {
                requestid = data.SupplyManagementShipment.SupplyManagementRequestId;
                shipmentid = data.SupplyManagementShipmentId;
                recieptid = id;
            }
            List<SupplyManagementReceiptHistoryGridDto> list = new List<SupplyManagementReceiptHistoryGridDto>();
            list.Add(_context.SupplyManagementRequest.Where(x => x.Id == requestid).Select(x => new SupplyManagementReceiptHistoryGridDto
            {
                Id = x.Id,
                ActivityBy = x.CreatedByUser.UserName,
                ActivityDate = x.CreatedDate,
                Status = "Requested",
                RequestQty = x.RequestQty,
                RequestType = x.IsSiteRequest ? "Site to Site" : "Site to Study",
                FromProjectCode = x.FromProject.ProjectCode,
                ToProjectCode = x.ToProject.ProjectCode,
                StudyProjectCode = _context.Project.Where(z => z.Id == x.FromProject.ParentProjectId).FirstOrDefault() != null ?
                                  _context.Project.Where(z => z.Id == x.FromProject.ParentProjectId).FirstOrDefault().ProjectCode : "",
                StudyProductTypeUnitName = x.PharmacyStudyProductType.ProductUnitType.GetDescription(),
                ProductTypeName = x.PharmacyStudyProductType.ProductType.ProductTypeName,
                VisitName = x.ProjectDesignVisit.DisplayName
            }).FirstOrDefault());

            list.Add(_context.SupplyManagementShipment.Where(x => x.Id == shipmentid).Select(x => new SupplyManagementReceiptHistoryGridDto
            {
                Id = x.Id,
                ActivityBy = x.CreatedByUser.UserName,
                ActivityDate = x.CreatedDate,
                Status = x.Status.GetDescription(),
                RequestType = x.SupplyManagementRequest.IsSiteRequest ? "Site to Site" : "Site to Study",
                FromProjectCode = x.SupplyManagementRequest.FromProject.ProjectCode,
                ToProjectCode = x.SupplyManagementRequest.ToProject.ProjectCode,
                StudyProjectCode = _context.Project.Where(z => z.Id == x.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault() != null ?
                                 _context.Project.Where(z => z.Id == x.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault().ProjectCode : "",
                StudyProductTypeUnitName = x.SupplyManagementRequest.PharmacyStudyProductType.ProductUnitType.GetDescription(),
                ProductTypeName = x.SupplyManagementRequest.PharmacyStudyProductType.ProductType.ProductTypeName,
                RequestQty = x.ApprovedQty,
                VisitName = x.SupplyManagementRequest.ProjectDesignVisit.DisplayName
            }).FirstOrDefault());
            if (recieptid > 0)
            {
                list.Add(All.Include(x => x.SupplyManagementShipment).Where(x => x.Id == id).Select(x => new SupplyManagementReceiptHistoryGridDto
                {
                    Id = x.Id,
                    ActivityBy = x.CreatedByUser.UserName,
                    ActivityDate = x.CreatedDate,
                    Status = "Receipt",
                    RequestType = x.SupplyManagementShipment.SupplyManagementRequest.IsSiteRequest ? "Site to Site" : "Site to Study",
                    FromProjectCode = x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode,
                    ToProjectCode = x.SupplyManagementShipment.SupplyManagementRequest.ToProject.ProjectCode,
                    StudyProjectCode = _context.Project.Where(z => z.Id == x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault() != null ?
                                    _context.Project.Where(z => z.Id == x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault().ProjectCode : "",
                    StudyProductTypeUnitName = x.SupplyManagementShipment.SupplyManagementRequest.PharmacyStudyProductType.ProductUnitType.GetDescription(),
                    ProductTypeName = x.SupplyManagementShipment.SupplyManagementRequest.PharmacyStudyProductType.ProductType.ProductTypeName,
                    RequestQty = _context.SupplyManagementKITDetail.Where(z => z.SupplyManagementShipmentId == x.SupplyManagementShipmentId && (z.Status == KitStatus.WithoutIssue || z.Status == KitStatus.WithIssue)
                                 && z.DeletedDate == null).Count(),
                    VisitName = x.SupplyManagementShipment.SupplyManagementRequest.ProjectDesignVisit.DisplayName
                }).FirstOrDefault());
            }
            return list.OrderByDescending(x => x.ActivityDate).ToList();
        }

        public List<KitAllocatedList> GetKitAllocatedList(int id, string Type)
        {
            if (Type == "Shipment")
            {
                var obj = _context.SupplyManagementShipment.Where(x => x.Id == id).FirstOrDefault();
                if (obj == null)
                    return new List<KitAllocatedList>();
                return _context.SupplyManagementKITDetail.Where(x =>
                          x.SupplyManagementShipmentId == id
                          && x.DeletedDate == null).Select(x => new KitAllocatedList
                          {
                              Id = x.Id,
                              KitNo = x.KitNo,
                              VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                              SiteCode = x.SupplyManagementKIT.Site.ProjectCode,
                              Comments = x.Comments,
                              Status = KitStatus.Allocated.ToString()
                          }).OrderByDescending(x => x.KitNo).ToList();

            }
            if (Type == "Receipt")
            {
                var obj = _context.SupplyManagementReceipt.Where(x => x.Id == id).FirstOrDefault();
                if (obj == null)
                    return new List<KitAllocatedList>();

                return _context.SupplyManagementKITDetail.Where(x =>
                        x.SupplyManagementShipmentId == obj.SupplyManagementShipmentId
                        && x.DeletedDate == null).Select(x => new KitAllocatedList
                        {
                            Id = x.Id,
                            KitNo = x.KitNo,
                            VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                            SiteCode = x.SupplyManagementKIT.Site.ProjectCode,
                            Comments = x.Comments,
                            Status = x.Status.GetDescription()
                        }).OrderByDescending(x => x.KitNo).ToList();
            }
            return new List<KitAllocatedList>(); ;

        }
    }
}

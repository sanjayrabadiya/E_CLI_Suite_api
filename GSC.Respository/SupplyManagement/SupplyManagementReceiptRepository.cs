﻿using AutoMapper;
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
        private readonly ISupplyManagementKITDetailRepository _supplyManagementKITDetailRepository;
        private readonly ISupplyManagementKITRepository _supplyManagementKITRepository;

        public SupplyManagementReceiptRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, ISupplyManagementKITDetailRepository supplyManagementKITDetailRepository, ISupplyManagementKITRepository supplyManagementKITRepository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _supplyManagementKITDetailRepository = supplyManagementKITDetailRepository;
            _supplyManagementKITRepository = supplyManagementKITRepository;
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
                    t.ProjectId = study.ParentProjectId;
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
                
                var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                if (fromproject != null)
                {
                    var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                    obj.StudyProjectCode = study != null ? study.ProjectCode : "";
                    obj.ProjectId = study.Id;
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
                ProjectId = x.FromProject.ParentProjectId,
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
                ProjectId = x.SupplyManagementRequest.FromProject.ParentProjectId,
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
                    ProjectId = x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ParentProjectId,
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
                var obj = _context.SupplyManagementShipment.Include(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x => x.Id == id).FirstOrDefault();
                if (obj == null)
                    return new List<KitAllocatedList>();

                var settings = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == obj.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault();
                if (settings.KitCreationType == KitCreationType.KitWise)
                {
                    return _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x =>
                                    x.SupplyManagementShipmentId == id
                                    && x.DeletedDate == null).Select(x => new KitAllocatedList
                                    {
                                        Id = x.Id,
                                        KitNo = x.KitNo,
                                        VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                        SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : "",
                                        Comments = x.Comments,
                                        Status = KitStatus.Shipped.ToString()
                                    }).OrderByDescending(x => x.KitNo).ToList();
                }
                else
                {
                    return _context.SupplyManagementKITSeries.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x =>
                                    x.SupplyManagementShipmentId == id
                                    && x.DeletedDate == null).Select(x => new KitAllocatedList
                                    {
                                        Id = x.Id,
                                        KitNo = x.KitNo,
                                        SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : "",
                                        Comments = x.Comments,
                                        Status = KitStatus.Shipped.ToString()
                                    }).OrderByDescending(x => x.KitNo).ToList();
                }

            }
            if (Type == "Receipt")
            {
                var obj = _context.SupplyManagementReceipt.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x => x.Id == id).FirstOrDefault();
                if (obj == null)
                    return new List<KitAllocatedList>();
                var settings = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == obj.SupplyManagementShipment.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault();
                if (settings.KitCreationType == KitCreationType.KitWise)
                {

                    return _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x =>
                        x.SupplyManagementShipmentId == obj.SupplyManagementShipmentId
                        && x.DeletedDate == null).Select(x => new KitAllocatedList
                        {
                            Id = x.Id,
                            KitNo = x.KitNo,
                            VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                            SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : "",
                            Comments = x.Comments,
                            Status = x.Status.GetDescription()
                        }).OrderByDescending(x => x.KitNo).ToList();
                }
                else
                {
                    return _context.SupplyManagementKITSeries.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x =>
                        x.SupplyManagementShipmentId == obj.SupplyManagementShipmentId
                        && x.DeletedDate == null).Select(x => new KitAllocatedList
                        {
                            Id = x.Id,
                            KitNo = x.KitNo,
                            SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : "",
                            Comments = x.Comments,
                            Status = x.Status.GetDescription()
                        }).OrderByDescending(x => x.KitNo).ToList();
                }
            }
            return new List<KitAllocatedList>();

        }
        public string CheckValidationKitReciept(SupplyManagementReceiptDto supplyManagementshipmentDto)
        {
            string Message = string.Empty;
            if (supplyManagementshipmentDto.Kits != null)
            {

                foreach (var item in supplyManagementshipmentDto.Kits)
                {
                    if (item.Status == 0)
                    {
                        Message = "Please select kit type! at kit no " + item.KitNo;
                        return Message;
                    }
                    if (item.Status != Helper.KitStatus.WithoutIssue)
                    {
                        if (string.IsNullOrEmpty(item.Comments))
                        {
                            Message = "Please enter comments! " + item.KitNo;
                            return Message;
                        }
                    }
                }
            }
            return Message;
        }

        public void UpdateKitStatus(SupplyManagementReceiptDto supplyManagementshipmentDto, SupplyManagementShipment supplyManagementShipment)
        {
            var settings = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == supplyManagementShipment.SupplyManagementRequest.FromProject.ParentProjectId && x.DeletedDate == null).FirstOrDefault();
            if (settings.KitCreationType == KitCreationType.KitWise)
            {
                if (supplyManagementshipmentDto.Kits != null)
                {
                    foreach (var item in supplyManagementshipmentDto.Kits)
                    {
                        var data = _supplyManagementKITDetailRepository.All.Where(x => x.Id == item.Id).FirstOrDefault();
                        if (data != null)
                        {
                            data.Status = item.Status;
                            data.Comments = item.Comments;
                            _supplyManagementKITDetailRepository.Update(data);
                        }
                    }
                }

                if (supplyManagementshipmentDto.Kits != null)
                {
                    foreach (var item in supplyManagementshipmentDto.Kits)
                    {
                        SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                        history.SupplyManagementKITDetailId = item.Id;
                        history.Status = item.Status;
                        history.RoleId = _jwtTokenAccesser.RoleId;
                        _supplyManagementKITRepository.InsertKitHistory(history);
                    }
                }
            }
            if (settings.KitCreationType == KitCreationType.SequenceWise)
            {
                if (supplyManagementshipmentDto.Kits != null)
                {

                    foreach (var item in supplyManagementshipmentDto.Kits)
                    {
                        var data = _context.SupplyManagementKITSeries.Where(x => x.Id == item.Id).FirstOrDefault();
                        if (data != null)
                        {
                            data.Status = item.Status;
                            data.Comments = item.Comments;
                            _context.SupplyManagementKITSeries.Update(data);
                        }
                    }
                }

                if (supplyManagementshipmentDto.Kits != null)
                {
                    foreach (var item in supplyManagementshipmentDto.Kits)
                    {
                        SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                        history.SupplyManagementKITSeriesId = item.Id;
                        history.Status = item.Status;
                        history.RoleId = _jwtTokenAccesser.RoleId;
                        _supplyManagementKITRepository.InsertKitSequenceHistory(history);
                    }
                }
            }
        }
    }
}

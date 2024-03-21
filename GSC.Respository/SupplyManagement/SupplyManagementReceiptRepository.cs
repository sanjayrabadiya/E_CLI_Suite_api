using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
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

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementReceiptRepository : GenericRespository<SupplyManagementReceipt>, ISupplyManagementReceiptRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly ISupplyManagementKitDetailRepository _supplyManagementKITDetailRepository;
        private readonly ISupplyManagementKitRepository _supplyManagementKITRepository;

        public SupplyManagementReceiptRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, ISupplyManagementKitDetailRepository supplyManagementKITDetailRepository, ISupplyManagementKitRepository supplyManagementKITRepository)
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
            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                         Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == parentProjectId
                         && s.RoleId == _jwtTokenAccesser.RoleId);

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == parentProjectId).FirstOrDefault();

            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == SiteId
            ).ProjectTo<SupplyManagementReceiptGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            data.ForEach(t =>
            {
                var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                if (fromproject != null)
                {
                    var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                    if (study != null)
                    {
                        t.StudyProjectCode = study.ProjectCode;
                        t.ProjectId = study.ParentProjectId;
                    }
                }
                t.WithIssueName = t.WithIssue == true ? "Yes" : "No";
                t.StudyProductTypeName = setting != null && setting.IsBlindedStudy == true && isShow ? "" : t.StudyProductTypeName;
                if (t.RoleId > 0)
                {
                    var role = _context.SecurityRole.Where(s => s.Id == t.RoleId).FirstOrDefault();
                    if (role != null)
                        t.RoleName = role.RoleName;
                }
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
                obj.StudyProductTypeName = setting != null && setting.IsBlindedStudy == true && isShow ? "" : t.StudyProductTypeName;
                var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                if (fromproject != null)
                {
                    var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                    if (study != null)
                    {
                        t.StudyProjectCode = study.ProjectCode;
                        t.ProjectId = study.Id;
                    }
                }
                obj.IpAddress = t.IpAddress;
                obj.TimeZone = t.TimeZone;
                data.Add(obj);
            });
            return data.OrderByDescending(x => x.ApproveRejectDateTime).ToList();
        }

        public List<SupplyManagementReceiptHistoryGridDto> GetSupplyShipmentReceiptHistory(int id)
        {
            int requestid = 0;
            int shipmentid = 0;
            int recieptid = 0;
            int? projectId = 0;
            var data = All.Include(x => x.SupplyManagementShipment).Where(x => x.Id == id).FirstOrDefault();
            if (data == null)
            {
                var data1 = _context.SupplyManagementShipment.Include(x => x.SupplyManagementRequest).Where(x => x.Id == id).FirstOrDefault();
                if (data1 != null)
                {
                    requestid = data1.SupplyManagementRequestId;
                    shipmentid = data1.Id;
                }

            }
            else
            {
                requestid = data.SupplyManagementShipment.SupplyManagementRequestId;
                shipmentid = data.SupplyManagementShipmentId;
                recieptid = id;
                projectId = _context.SupplyManagementRequest.Include(s => s.FromProject).Where(s => s.Id == requestid).Select(s => s.FromProject.ParentProjectId).FirstOrDefault();
            }

            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                         Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == projectId
                         && s.RoleId == _jwtTokenAccesser.RoleId);

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == projectId && x.DeletedDate == null).FirstOrDefault();
            if (setting == null)
            {
                return new List<SupplyManagementReceiptHistoryGridDto>();
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
                ProductTypeName = setting != null && setting.IsBlindedStudy == true && isShow ? "" : x.PharmacyStudyProductType.ProductType.ProductTypeName,
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
                ProductTypeName = setting != null && setting.IsBlindedStudy == true && isShow ? "" : x.SupplyManagementRequest.PharmacyStudyProductType.ProductType.ProductTypeName,
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
                    ProductTypeName = setting != null && setting.IsBlindedStudy == true && isShow ? "" : x.SupplyManagementShipment.SupplyManagementRequest.PharmacyStudyProductType.ProductType.ProductTypeName,
                    RequestQty = setting.KitCreationType == KitCreationType.KitWise ? _context.SupplyManagementKITDetail.Where(z => z.SupplyManagementShipmentId == x.SupplyManagementShipmentId && z.DeletedDate == null).Count() :
                                 _context.SupplyManagementKITSeries.Where(z => z.SupplyManagementShipmentId == x.SupplyManagementShipmentId && z.DeletedDate == null).Count(),
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
                var isShow = _context.SupplyManagementKitNumberSettingsRole.
                             Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == obj.SupplyManagementRequest.FromProject.ParentProjectId
                             && s.RoleId == _jwtTokenAccesser.RoleId);

                var settings = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == obj.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault();
                if (settings != null && settings.KitCreationType == KitCreationType.KitWise)
                {
                    return _context.SupplyManagementKITDetail.Include(s => s.SupplyManagementKIT).ThenInclude(s => s.PharmacyStudyProductType).ThenInclude(s => s.ProductType).Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x =>
                                    x.SupplyManagementShipmentId == id
                                    && x.DeletedDate == null).Select(x => new KitAllocatedList
                                    {
                                        Id = x.Id,
                                        KitNo = x.KitNo,
                                        VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                        SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null && !x.SupplyManagementShipment.SupplyManagementRequest.IsSiteRequest ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : x.SupplyManagementShipment.SupplyManagementRequest.ToProject.ProjectCode,
                                        Comments = x.Comments,
                                        Status = KitStatus.Shipped.ToString(),
                                        ProductTypeName = settings.IsBlindedStudy == true && isShow ? "" : x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode
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
                                        SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null && !x.SupplyManagementShipment.SupplyManagementRequest.IsSiteRequest ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : x.SupplyManagementShipment.SupplyManagementRequest.ToProject.ProjectCode,
                                        Comments = x.Comments,
                                        Status = KitStatus.Shipped.ToString(),
                                        ProductTypeName = settings.IsBlindedStudy == true && isShow ? "" : x.TreatmentType
                                    }).OrderByDescending(x => x.KitNo).ToList();
                }

            }
            if (Type == "Receipt")
            {
                var obj = _context.SupplyManagementReceipt.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x => x.Id == id).FirstOrDefault();
                if (obj == null)
                    return new List<KitAllocatedList>();
                var isShow = _context.SupplyManagementKitNumberSettingsRole.
                          Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == obj.SupplyManagementShipment.SupplyManagementRequest.FromProject.ParentProjectId
                          && s.RoleId == _jwtTokenAccesser.RoleId);

                var settings = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == obj.SupplyManagementShipment.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault();
                if (settings != null && settings.KitCreationType == KitCreationType.KitWise)
                {

                    return _context.SupplyManagementKITDetail.Include(s => s.SupplyManagementKIT).ThenInclude(s => s.PharmacyStudyProductType).ThenInclude(s => s.ProductType).Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x =>
                        x.SupplyManagementShipmentId == obj.SupplyManagementShipmentId
                        && x.DeletedDate == null).Select(x => new KitAllocatedList
                        {
                            Id = x.Id,
                            KitNo = x.KitNo,
                            VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                            SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : "",
                            Comments = x.Comments,
                            Status = x.Status.GetDescription(),
                            ProductTypeName = settings.IsBlindedStudy == true && isShow ? "" : x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode
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
                            Status = x.Status.GetDescription(),
                            ProductTypeName = settings.IsBlindedStudy == true && isShow ? "" : x.TreatmentType
                        }).OrderByDescending(x => x.KitNo).ToList();
                }
            }
            return new List<KitAllocatedList>();

        }
        public string CheckValidationKitReciept(SupplyManagementReceiptDto supplyManagementshipmentDto)
        {
            string Message = string.Empty;
            var shipment = _context.SupplyManagementShipment.Include(s => s.SupplyManagementRequest).ThenInclude(s => s.FromProject).Where(s => s.Id == supplyManagementshipmentDto.SupplyManagementShipmentId).FirstOrDefault();
            if (shipment == null)
            {
                Message = "Shipment not found";
                return Message;
            }

            if (supplyManagementshipmentDto.Kits != null)
            {

                foreach (var item in supplyManagementshipmentDto.Kits)
                {
                    if (item.Status == 0)
                    {
                        Message = "Please select kit type! at kit no " + item.KitNo;
                        return Message;
                    }
                    if (item.Status != Helper.KitStatus.WithoutIssue && string.IsNullOrEmpty(item.Comments))
                    {
                        Message = "Please enter comments! " + item.KitNo;
                        return Message;

                    }
                    Message = CheckExpiryOnReceipt((int)shipment.SupplyManagementRequest.FromProject.ParentProjectId, item.Id);
                    if (!string.IsNullOrEmpty(Message))
                    {
                        return Message;
                    }

                }
            }
            return Message;
        }

        public void UpdateKitStatus(SupplyManagementReceiptDto supplyManagementshipmentDto, SupplyManagementShipment supplyManagementShipment)
        {
            var request = _context.SupplyManagementRequest.Where(x => x.Id == supplyManagementShipment.SupplyManagementRequestId).FirstOrDefault();
            var settings = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == supplyManagementShipment.SupplyManagementRequest.FromProject.ParentProjectId && x.DeletedDate == null).FirstOrDefault();
            if (settings != null && settings.KitCreationType == KitCreationType.KitWise)
            {
                if (supplyManagementshipmentDto.Kits != null)
                {
                    foreach (var item in supplyManagementshipmentDto.Kits)
                    {
                        var data = _supplyManagementKITDetailRepository.All.Include(x => x.SupplyManagementKIT).Where(x => x.Id == item.Id).FirstOrDefault();
                        if (data != null)
                        {
                            data.Status = item.Status;
                            data.Comments = item.Comments;
                            data.IsRetension = item.IsRetension;
                            _context.SupplyManagementKITDetail.Update(data);
                            _context.Save();

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
                        var data = _supplyManagementKITDetailRepository.All.Include(x => x.SupplyManagementKIT).Where(x => x.Id == item.Id).FirstOrDefault();
                        if (data != null)
                        {
                            history.SupplyManagementShipmentId = data.SupplyManagementShipmentId;
                        }
                        _supplyManagementKITRepository.InsertKitHistory(history);
                    }
                }
            }
            if (settings != null && settings.KitCreationType == KitCreationType.SequenceWise)
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
                            data.IsRetension = item.IsRetension;
                            if (request != null && request.IsSiteRequest)
                            {
                                data.ToSiteId = request.FromProjectId;
                            }
                            _context.SupplyManagementKITSeries.Update(data);
                        }

                    }
                    _context.Save();
                }

                if (supplyManagementshipmentDto.Kits != null)
                {
                    foreach (var item in supplyManagementshipmentDto.Kits)
                    {
                        SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                        history.SupplyManagementKITSeriesId = item.Id;
                        var data = _context.SupplyManagementKITSeries.Where(x => x.Id == item.Id).FirstOrDefault();
                        if (data != null)
                        {
                            history.SupplyManagementShipmentId = data.SupplyManagementShipmentId;
                        }
                        history.Status = item.Status;
                        history.RoleId = _jwtTokenAccesser.RoleId;
                        _supplyManagementKITRepository.InsertKitSequenceHistory(history);
                    }
                }
            }

        }

        public string CheckExpiryOnReceipt(int projectId, int id)
        {

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == projectId).FirstOrDefault();
            if (setting != null)
            {

                if (setting.KitCreationType == KitCreationType.KitWise)
                {
                    var kit = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).Where(x => x.Id == id).FirstOrDefault();
                    if (kit != null)
                    {
                        var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == kit.SupplyManagementKIT.ProductReceiptId).FirstOrDefault();
                        if (productreciept == null)
                            return "Product receipt not found";
                        var currentdate = DateTime.Now.Date;
                        var expiry = Convert.ToDateTime(productreciept.RetestExpiryDate).Date;
                        var date = expiry.AddDays(-(int)kit.SupplyManagementKIT.Days);
                        if (currentdate.Date > date.Date)
                        {
                            return "Product is expired for this kit " + kit.KitNo;
                        }

                    }

                }
                if (setting.KitCreationType == KitCreationType.SequenceWise)
                {
                    var kit = _context.SupplyManagementKITSeries.Where(x => x.Id == id).FirstOrDefault();
                    if (kit != null)
                    {
                        var currentdate = DateTime.Now.Date;
                        if (Convert.ToDateTime(kit.KitExpiryDate).Date < currentdate.Date)
                        {
                            return "Product is expired for this kit " + kit.KitNo;
                        }
                    }
                }
            }

            return "";
        }
    }
}

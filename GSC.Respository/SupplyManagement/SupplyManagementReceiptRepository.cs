using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
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
        public List<SupplyManagementReceiptGridDto> GetSupplyShipmentReceiptList(bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                    ProjectTo<SupplyManagementReceiptGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            if (data == null)
            {
                data = new List<SupplyManagementReceiptGridDto>();
            }
            else
            {
                data.ForEach(t =>
                {
                    var toproject = _context.Project.Where(x => x.Id == t.ToProjectId).FirstOrDefault();
                    if (toproject != null)
                    {
                        t.ToProjectCode = toproject.ProjectCode;

                    }
                    var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                    if (fromproject != null)
                    {
                        var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                        t.StudyProjectCode = study != null ? study.ProjectCode : "";
                    }
                });
            }

            var requestdata = _context.SupplyManagementShipment.Where(x =>
                x.Status == SupplyMangementShipmentStatus.Approve && x.DeletedDate == null).
                 ProjectTo<SupplyManagementShipmentGridDto>(_mapper.ConfigurationProvider).ToList();
            requestdata.ForEach(t =>
            {
                if (!data.Any(x => x.SupplyManagementShipmentId == t.Id))
                {
                    SupplyManagementReceiptGridDto obj = new SupplyManagementReceiptGridDto();
                    obj.ApprovedQty = t.ApprovedQty;
                    obj.ApproveRejectDateTime = t.CreatedDate;
                    obj.SupplyManagementShipmentId = t.Id;
                    obj.FromProjectCode = t.FromProjectCode;
                    obj.ShipmentNo = t.ShipmentNo;
                    obj.FromProjectId = t.FromProjectId;
                    obj.ToProjectId = t.ToProjectId;
                    obj.CourierName = t.CourierName;
                    obj.CourierDate = t.CourierDate;
                    obj.CourierTrackingNo = t.CourierTrackingNo;
                    obj.Status = t.Status;
                    var toproject = _context.Project.Where(x => x.Id == t.ToProjectId).FirstOrDefault();
                    if (toproject != null)
                    {
                        obj.ToProjectCode = toproject.ProjectCode;

                    }
                    var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                    if (fromproject != null)
                    {
                        var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                        obj.StudyProjectCode = study != null ? study.ProjectCode : "";
                    }
                    data.Add(obj);
                }
            });
            return data.OrderByDescending(x => x.CreatedDate).ToList();
        }
    }
}

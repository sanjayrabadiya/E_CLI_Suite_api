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
                var shipmentdata = _context.SupplyManagementShipment.Where(x => x.SupplyMangementRequestId == t.Id).FirstOrDefault();
                if (shipmentdata != null)
                {
                    t.ApprovedQty = shipmentdata.ApprovedQty;
                    t.SupplyManagementShipmentId = shipmentdata.Id;
                    t.Status = shipmentdata.Status.GetDescription();
                    t.ApproveRejectDateTime = shipmentdata.CreatedDate;
                    t.AuditReason = _context.AuditReason.Where(x => x.Id == shipmentdata.AuditReasonId).FirstOrDefault() != null ?
                    _context.AuditReason.Where(x => x.Id == shipmentdata.AuditReasonId).FirstOrDefault().ReasonName : "";
                    t.ReasonOth = shipmentdata.ReasonOth;
                }
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

            return data;
        }

    }
}

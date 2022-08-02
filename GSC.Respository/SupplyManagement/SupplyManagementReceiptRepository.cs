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
    }
}

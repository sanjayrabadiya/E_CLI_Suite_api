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
    public class SupplyManagementShipmentRepository : GenericRespository<SupplyManagementShipment>, ISupplyManagementShipmentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private static Random random = new Random();
        public SupplyManagementShipmentRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }
        public List<SupplyManagementShipmentGridDto> GetSupplyShipmentList(bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                    ProjectTo<SupplyManagementShipmentGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            data.ForEach(t =>
            {
                var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                if (fromproject != null)
                {
                    var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                    t.StudyProjectCode = study != null ? study.ProjectCode : "";
                }
            });



            var requestdata = _context.SupplyManagementRequest.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                     ProjectTo<SupplyManagementRequestGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            requestdata.ForEach(t =>
            {
                if (data != null && !data.Any(x => x.SupplyManagementRequestId == t.Id))
                {
                    SupplyManagementShipmentGridDto obj = new SupplyManagementShipmentGridDto();
                    obj.IsSiteRequest = t.IsSiteRequest;
                    obj.FromProjectId = t.FromProjectId;
                    obj.ToProjectId = t.ToProjectId;
                    obj.SupplyManagementRequestId = t.Id;
                    obj.RequestQty = t.RequestQty;
                    obj.FromProjectCode = t.FromProjectCode;
                    obj.ToProjectCode = t.ToProjectCode;
                    obj.CreatedByUser = t.CreatedByUser;
                    obj.ModifiedByUser = t.ModifiedByUser;
                    obj.CreatedDate = t.CreatedDate;
                    obj.DeletedDate = t.DeletedDate;
                    obj.DeletedByUser = t.DeletedByUser;
                    obj.IsDeleted = t.IsDeleted;
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

        public string GenerateShipmentNo()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

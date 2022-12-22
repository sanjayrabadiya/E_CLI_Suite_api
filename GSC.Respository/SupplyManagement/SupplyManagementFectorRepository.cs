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
    public class SupplyManagementFectorRepository : GenericRespository<SupplyManagementFector>, ISupplyManagementFectorRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementFectorRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public SupplyManagementFectorDto GetById(int id)
        {
            var data = _context.SupplyManagementFector.Where(x => x.Id == id && x.DeletedDate == null).Select(x => new SupplyManagementFectorDto
            {
                Id = x.Id,
                ProjectId = x.ProjectId,
                ProjectCode = x.Project.ProjectCode,
                Formula = x.Formula,
                Children = x.FectorDetailList.Where(x => x.DeletedDate == null).Select(z => new SupplyManagementFectorDetailDto
                {
                    Id = z.Id,
                    ProductTypeCode = z.ProductTypeCode,
                    Fector = z.Fector,
                    Operator = z.Operator,
                    CollectionValue = z.CollectionValue,
                    LogicalOperator = z.LogicalOperator,
                    Ratio = z.Ratio,
                    FactoreName = z.Fector.GetDescription(),
                    FactoreOperatorName = z.Operator.ToString(),
                    collectionValueName = z.CollectionValue,
                    Type = z.Type,
                    TypeName = z.Type.GetDescription(),
                }).ToList()

            }).FirstOrDefault();

            return data;
        }

        public List<SupplyManagementFectorGridDto> GetListByProjectId(int projectId, bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == projectId).
                    ProjectTo<SupplyManagementFectorGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            //var data = _context.SupplyManagementFector.Where(x => x.ProjectId == projectId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).Select(x => new SupplyManagementFectorGridDto
            //{
            //    Id = x.Id,
            //    ProjectId = x.ProjectId,
            //    ProjectCode = x.Project.ProjectCode,
            //    Formula = x.Formula,
            //    DeletedDate = x.DeletedDate,
            //    CreatedDate = x.CreatedDate,
            //    ModifiedDate = x.ModifiedDate,
            //    CreatedByUser = _context.Users.Where(x => x.Id == x.CreatedBy).FirstOrDefault().UserName,
            //    ModifiedByUser = x.ModifiedBy > 0 ? _context.Users.Where(x => x.Id == x.ModifiedBy).FirstOrDefault().UserName : "",
            //    DeletedByUser = _context.Users.Where(x => x.Id == x.DeletedBy).FirstOrDefault().UserName,
            //    IsDeleted = x.DeletedDate != null ? true : false
            //}).ToList();

            return data;
        }


        public void DeleteChild(int Id)
        {
            var list = _context.SupplyManagementFectorDetail.Where(x => x.Id == Id).ToList();
            _context.SupplyManagementFectorDetail.RemoveRange(list);
            _context.Save();
        }
    }
}

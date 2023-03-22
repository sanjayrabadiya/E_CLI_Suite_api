using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementFactorMappingRepository : GenericRespository<SupplyManagementFactorMapping>, ISupplyManagementFactorMappingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SupplyManagementFactorMappingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementFactorMappingGridDto> GetSupplyFactorMappingList(bool isDeleted, int ProjectId)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementFactorMappingGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
       
    }
}

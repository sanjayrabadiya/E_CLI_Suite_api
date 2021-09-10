using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.LabManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.LabManagement
{
    public class LabManagementVariableMappingRepository : GenericRespository<LabManagementVariableMapping>, ILabManagementVariableMappingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public LabManagementVariableMappingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

    }
}

using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class VerificationApprovalTemplateValueRepository : GenericRespository<VerificationApprovalTemplateValue>, IVerificationApprovalTemplateValueRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public VerificationApprovalTemplateValueRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }
    }
}

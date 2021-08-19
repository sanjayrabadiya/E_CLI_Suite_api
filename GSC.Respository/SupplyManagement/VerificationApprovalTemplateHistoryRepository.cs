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
    public class VerificationApprovalTemplateHistoryRepository : GenericRespository<VerificationApprovalTemplateHistory>, IVerificationApprovalTemplateHistoryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IVerificationApprovalTemplateRepository _verificationApprovalTemplateRepository;

        public VerificationApprovalTemplateHistoryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IVerificationApprovalTemplateRepository verificationApprovalTemplateRepository,
        IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _verificationApprovalTemplateRepository = verificationApprovalTemplateRepository;
            _mapper = mapper;
        }
    }
}

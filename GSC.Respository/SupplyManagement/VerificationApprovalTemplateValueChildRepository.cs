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
    public class VerificationApprovalTemplateValueChildRepository : GenericRespository<VerificationApprovalTemplateValueChild>, IVerificationApprovalTemplateValueChildRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public VerificationApprovalTemplateValueChildRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }
        public void Save(VerificationApprovalTemplateValue verificationApprovalTemplateValue)
        {
            if (verificationApprovalTemplateValue.Children != null)
            {
                verificationApprovalTemplateValue.Children.ForEach(x =>
                {
                    if (x.Id == 0)
                        Add(x);
                    else
                        Update(x);
                });
            }
        }
    }
}

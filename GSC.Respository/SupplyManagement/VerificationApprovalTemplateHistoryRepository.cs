using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class VerificationApprovalTemplateHistoryRepository : GenericRespository<VerificationApprovalTemplateHistory>, IVerificationApprovalTemplateHistoryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IVerificationApprovalTemplateRepository _verificationApprovalTemplateRepository;
        private readonly IGSCContext _context;
        public VerificationApprovalTemplateHistoryRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IVerificationApprovalTemplateRepository verificationApprovalTemplateRepository,
        IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _verificationApprovalTemplateRepository = verificationApprovalTemplateRepository;
            _mapper = mapper;
            _context = context;
        }


        public List<VerificationApprovalTemplateHistoryViewDto> GetHistoryByVerificationDetail(int ProductVerificationDetailId)
        {
            var data = All.Where(x => x.DeletedDate == null).Where(x => x.VerificationApprovalTemplate.ProductVerificationDetail.Id == ProductVerificationDetailId).
                   ProjectTo<VerificationApprovalTemplateHistoryViewDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                x.SendByRole = x.SendBySecurityRoleId > 0 ? _context.SecurityRole.Where(z => z.Id == x.SendBySecurityRoleId).FirstOrDefault().RoleName : "";

            });

            return data;
        }
    }
}

using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using System.Collections.Generic;
using System.Linq;


namespace GSC.Respository.SupplyManagement
{
    public class VerificationApprovalTemplateHistoryRepository : GenericRespository<VerificationApprovalTemplateHistory>, IVerificationApprovalTemplateHistoryRepository
    {
       
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public VerificationApprovalTemplateHistoryRepository(IGSCContext context,
            
        IMapper mapper)
            : base(context)
        {
          
            _mapper = mapper;
            _context = context;
        }


        public List<VerificationApprovalTemplateHistoryViewDto> GetHistoryByVerificationDetail(int ProductVerificationDetailId)
        {
            var data = All.Where(x => x.DeletedDate == null).Where(x => x.VerificationApprovalTemplate.ProductVerificationDetail.Id == ProductVerificationDetailId).
                   ProjectTo<VerificationApprovalTemplateHistoryViewDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                var role = _context.SecurityRole.Where(z => z.Id == x.SendBySecurityRoleId).FirstOrDefault();
                if (role != null)
                    x.SendByRole = role.RoleName;

            });

            return data;
        }
    }
}

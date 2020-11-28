using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class AuditReasonRepository : GenericRespository<AuditReason>, IAuditReasonRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public AuditReasonRepository(IGSCContext context,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        public List<DropDownDto> GetAuditReasonDropDown(AuditModule auditModule)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null
                                                                                        && (x.ModuleId ==
                                                                                            AuditModule.Common ||
                                                                                            x.ModuleId == auditModule))
                .Select(c => new DropDownDto {Id = c.Id, Value = c.ReasonName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(AuditReason objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.ReasonName == objSave.ReasonName && x.ModuleId == objSave.ModuleId &&
                x.DeletedDate == null)) return "Duplicate Audit reason name : " + objSave.ReasonName;
            return "";
        }

        public List<AuditReasonGridDto> GetAuditReasonList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<AuditReasonGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}
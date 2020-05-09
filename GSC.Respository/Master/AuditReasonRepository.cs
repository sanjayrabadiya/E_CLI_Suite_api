using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class AuditReasonRepository : GenericRespository<AuditReason, GscContext>, IAuditReasonRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public AuditReasonRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
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
    }
}
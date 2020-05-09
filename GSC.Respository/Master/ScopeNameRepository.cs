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
    public class ScopeNameRepository : GenericRespository<ScopeName, GscContext>, IScopeNameRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ScopeNameRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetScopeNameDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.Name}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(ScopeName objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.Name == objSave.Name && x.DeletedDate == null))
                return "Duplicate Scope name : " + objSave.Name;
            return "";
        }
    }
}
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
    public class UnitRepository : GenericRespository<Unit, GscContext>, IUnitRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public UnitRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetUnitDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.UnitName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Unit objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.UnitName == objSave.UnitName && x.DeletedDate == null))
                return "Duplicate Unit name : " + objSave.UnitName;
            return "";
        }
    }
}
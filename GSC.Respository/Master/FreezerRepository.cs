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
    public class FreezerRepository : GenericRespository<Freezer, GscContext>, IFreezerRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public FreezerRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetFreezerDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.FreezerName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Freezer objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.FreezerName == objSave.FreezerName && x.DeletedDate == null))
                return "Duplicate Freezer name : " + objSave.FreezerName;

            return "";
        }
    }
}
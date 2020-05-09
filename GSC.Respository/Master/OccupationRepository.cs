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
    public class OccupationRepository : GenericRespository<Occupation, GscContext>, IOccupationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public OccupationRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetOccupationDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.OccupationName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Occupation objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.OccupationName == objSave.OccupationName && x.DeletedDate == null))
                return "Duplicate Occupation name : " + objSave.OccupationName;
            return "";
        }
    }
}
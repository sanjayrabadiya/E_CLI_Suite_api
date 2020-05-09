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
    public class RaceRepository : GenericRespository<Race, GscContext>, IRaceRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public RaceRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetRaceDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.RaceName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(Race objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.RaceName == objSave.RaceName && x.DeletedDate == null))
                return "Duplicate Race name : " + objSave.RaceName;
            return "";
        }
    }
}
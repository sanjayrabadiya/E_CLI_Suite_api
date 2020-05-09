using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Location;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Master
{
    public class StateRepository : GenericRespository<State, GscContext>, IStateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public StateRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetStateDropDown(int countryId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null &&
                    x.CountryId == countryId)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.StateName}).OrderBy(o => o.Value).ToList();
        }

        public string DuplicateState(State objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.StateName == objSave.StateName && x.DeletedDate == null))
                return "Duplicate State name : " + objSave.StateName;

            return "";
        }
    }
}
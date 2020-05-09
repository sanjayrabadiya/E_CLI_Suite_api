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
    public class PopulationTypeRepository : GenericRespository<PopulationType, GscContext>, IPopulationTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public PopulationTypeRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }


        public List<DropDownDto> GetPopulationTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.PopulationName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(PopulationType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.PopulationName == objSave.PopulationName && x.DeletedDate == null))
                return "Duplicate Population name : " + objSave.PopulationName;

            return "";
        }
    }
}
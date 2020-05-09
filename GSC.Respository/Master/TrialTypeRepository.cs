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
    public class TrialTypeRepository : GenericRespository<TrialType, GscContext>, ITrialTypeRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public TrialTypeRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<DropDownDto> GetTrialTypeDropDown()
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
                .Select(c => new DropDownDto {Id = c.Id, Value = c.TrialTypeName}).OrderBy(o => o.Value).ToList();
        }

        public string Duplicate(TrialType objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.TrialTypeName == objSave.TrialTypeName && x.DeletedDate == null))
                return "Duplicate TrialType code : " + objSave.TrialTypeName;
            return "";
        }
    }
}